

//Importing dependencies
require('dotenv').config();
const express = require('express');
const admin = require('firebase-admin');

//Initializing express
const app = express();
app.use(express.json()); //Allows server to read JSON from Unity

//Initializing Firebase Admin SDK
var serviceAccount;
try {
    serviceAccount = require('./firebase-admin-key.json');
} catch (e) {
    console.log("Service key not found, switching to Environment Variables");
}

if(!admin.apps.length){
    if(serviceAccount){
        console.log("Initializing Firebase with Local Key")
        admin.initializeApp({
            // @ts-ignore
            credential: admin.credential.cert(serviceAccount)
        });
    } else {
        console.log("Initializing Firebase with Environment Variables")
        admin.initializeApp({
            credential: admin.credential.cert({
                projectId: process.env.FIREBASE_PROJECT_ID,
                clientEmail: process.env.FIREBASE_CLIENT_EMAIL,
                privateKey: process.env.FIREBASE_PRIVATE_KEY?.replace(/\\n/g, '\n'),
            })
        });
    }
}

//Importing the controllers
const {verifyToken} = require('./authentication/authMiddleware');
const syncController = require('./controllers/syncController');
const leaderboardController = require('./controllers/leaderboardController');

//When a POST request is made to /api/sync, call the handleSync function
app.post('/api/sync', verifyToken, syncController.handleSync);

//Test to verify if the server is running
app.get('/', (req, res) => {
    res.send('WanderVerse middleware is running!');
});

app.get('/api/keys', verifyToken, (req, res) => {

    //Pulled directly form process.env
    const key=process.env.GAME_ENCRYPTION_KEY;
    const iv=process.env.GAME_ENCRYPTION_IV;

    //Don't let the game start if the keys are missing    
    if(!key || !iv){
        console.error("CRITICAL: Encryption keys missing in environment variables!");
        return res.status(500).send({status: "ERROR", message: "Encryption keys not found"});
    }

    res.status(200).send({
        status: "SUCCESS",
        key: key,
        iv: iv
    });
});

//GET leaderboard endpoint
app.get('/api/leaderboard', leaderboardController.getGlobalLeaderboard);

//app object is exported so Vercel can use it to start the server
module.exports = app;

//Local Testing
const PORT = process.env.PORT || 3000;
if(require.main === module){
    app.listen(PORT, () => {
        console.log(`Server is running locally on http://localhost:${PORT}`);
    });
}