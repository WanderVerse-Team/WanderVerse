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




