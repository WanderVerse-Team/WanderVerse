//@ts-check

const admin = require('firebase-admin');
const db = admin.firestore();

// Caching is handled on the client (Unity) side per device.
//@ts-ignore
exports.getGlobalLeaderboard = async (req, res) => {
    try {
        //Get the top 10 users ordered by XP, high to low
        const snapshot = await db.collection('users')
            .orderBy('xp', 'desc')
            .limit(10)
            .get();

        /** @type {{userName: string, xp: number}[]} */
        const topScores = [];
        snapshot.forEach(doc => {       //loop through each document in the snapshot
            const data = doc.data();
            topScores.push({            //Add a simplified object to the array
                userName: data.userName || "Unknown Explorer",
                xp: data.xp || 0,
            })
        });

        return res.status(200).send({
            status: 'SUCCESS',
            data: topScores
        });

    } catch (e) {
        console.error("Error fetching leaderboard:", e);
        res.status(500).send({ error: 'Internal Server Error' });
    }
}