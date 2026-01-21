//@ts-check

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
        admin.initializeApp();
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