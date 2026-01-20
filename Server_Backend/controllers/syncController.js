//@ts-check
const admin = require('firebase-admin');
const db = admin.firestore();

//exporting the sync handler function to be used in index.js
//@ts-ignore
exports.handleSync = async (req, res) => {
    try {
        //Take userId and localData from request body
        const { userId, localData } = req.body;

        if (!userId || !localData) {
            console.warn("Sync request failed: Missing userId or localData");
            return res.status(400).send({ error: 'Missing userId or localData' });
        }

        //Getting user document from Firestore
        const userRef = db.collection('users').doc(userId);
        const doc = await userRef.get();

        if (!doc.exists) {
            console.log(`Creating new Profile for user: ${userId}`);
            await userRef.set({
                ...localData, //spread operator to copy all fields from localData
                createdAt: admin.firestore.FieldValue.serverTimestamp(),
                lastSyncedAt: admin.firestore.FieldValue.serverTimestamp()
            });
            //Return created status 201
            return res.status(201).send({status: "SUCCESS", message: "New profile created."});
        }

        //conflict resolution logic
        const cloudData = doc.data(); //The data stored in Firestore

        //Rule is to never let an older save overwrite a newer or better one
        //@ts-ignore
        if(cloudData.xp > localData.xp){
            //@ts-ignore
            console.log(`Conflict detected for user ${userId}: cloud(${cloudData.xp}) > local(${localData.xp})`);

            return res.status(200).send({
                status: "CONFLICT",     //Status CONFLICT tells unity to discard its local files
                message: "Cloud data is ahead. Overwrite local",
                forceUpdate: cloudData //contains the better cloud data
            });
        }

        //Local data is newer or better, update cloud
        console.log(`Syncing success for user: ${userId}, updating cloud.`);

        await userRef.update({
            ...localData,
            lastSyncedAt: admin.firestore.FieldValue.serverTimestamp()
        });

        res.status(200).send({status: "SUCCESS", message: "Cloud data updated."});

    } catch (e) {
        console.error("Error during sync operation:", e);
        res.status(500).send({ error: 'Internal Server Error' });

    }
}
