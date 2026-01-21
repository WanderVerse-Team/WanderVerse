//@ts-check

const admin = require('firebase-admin');
const db = admin.firestore();

//@ts-ignore
let cachedLeaderboard = null;
let lastFetchTime = 0;

const cacheDuration = 5 * 60 * 1000; // 5 minutes

//@ts-ignore
exports.getGlobalLeaderboard = async (req, res) => {
    try {
        const currentTime = Date.now();

        //@ts-ignore
        if(cachedLeaderboard && (currentTime - lastFetchTime) < cacheDuration){
            console.log("Serving leaderboard from cache");
            return res.status(200).send({
                status: 'SUCCESS',
                source: 'cache',
                data: cachedLeaderboard
            });
        }

        console.log("Cache expired or empty, fetching live data from Firestore");

        //Get the top 10 users ordered by XP, high to low
        const snapshot = await db.collection('users')
            .orderBy('xp', 'desc')
            .limit(10)
            .get();

        //@ts-ignore
        const topScores = [];
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

        //send response
        res.status(200).send({
            status: 'SUCCESS',
            source: 'database',
            data: cachedLeaderboard
        })


    } catch (e) {
        console.error("Error fetching leaderboard:", e);
        res.status(500).send({ error: 'Internal Server Error' });
    }
}