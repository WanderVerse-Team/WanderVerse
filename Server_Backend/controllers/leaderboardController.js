//@ts-check

const admin = require('firebase-admin');
const db = admin.firestore();

/** @typedef {{ userName: string, xp: number }} LeaderboardEntry */

/** @type {LeaderboardEntry[] | null} */
let cachedLeaderboard = null;
let lastFetchTime = 0;

const cacheDuration = 5 * 60 * 1000; // 5 minutes

//@ts-ignore
exports.getGlobalLeaderboard = async (req, res) => {
    try {
        const currentTime = Date.now();

        // Fetch top 10 (use cache if fresh)
        /** @type {LeaderboardEntry[]} */
        let topScores = [];

        //@ts-ignore
        if (cachedLeaderboard && (currentTime - lastFetchTime) < cacheDuration) {
            console.log("Serving leaderboard from cache");
            topScores = cachedLeaderboard;
        } else {
            console.log("Cache expired or empty, fetching live data from Firestore");

            //Get the top 10 users ordered by XP, high to low
            const snapshot = await db.collection('users')
                .orderBy('xp', 'desc')
                .limit(10)
                .get();

            snapshot.forEach(doc => {       //loop through each document in the snapshot
                const data = doc.data();
                topScores.push({            //Add a simplified object to the array
                    userName: data.userName || "Unknown Explorer",
                    xp: data.xp || 0,
                })
            });

            //Update cache
            //@ts-ignore 
            cachedLeaderboard = topScores;
            lastFetchTime = currentTime;
        }

        // Find the current user's rank
        const userId = req.user.uid;
        const userDoc = await db.collection('users').doc(userId).get();

        let currentUser = null;

        if (userDoc.exists) {
            const userData = userDoc.data();
            //@ts-ignore
            const userXp = userData.xp || 0;
            //@ts-ignore
            const userNameVal = userData.userName || "Unknown Explorer";

            // Count how many users have more XP than this user
            const higherXpSnapshot = await db.collection('users')
                .where('xp', '>', userXp)
                .count()
                .get();

            const rank = higherXpSnapshot.data().count + 1;

            currentUser = {
                userName: userNameVal,
                xp: userXp,
                rank: rank,
            };
        }

        //send response
        return res.status(200).send({
            status: 'SUCCESS',
            source: cachedLeaderboard === topScores && (currentTime - lastFetchTime) < cacheDuration ? 'cache' : 'database',
            data: topScores,
            currentUser: currentUser,
        });

    } catch (e) {
        console.error("Error fetching leaderboard:", e);
        res.status(500).send({ error: 'Internal Server Error' });
    }
}