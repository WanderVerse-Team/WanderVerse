//@ts-check
const admin = require('firebase-admin');

//GateKeeper function
//@ts-ignore
exports.verifyToken = async (req, res, next) => {

    //standard format is "Authorization: Bearer <token>", so we split by 'Bearer ' to take just the token
    const idToken = req.headers.authorization?.split('Bearer ')[1];

    //Block the request if no token is provided
    if (!idToken) {    
        console.warn("Request Blocked: No token provided");
        return res.status(401).send({ error: 'Unauthorized: No token provided' });
    }

    try {

         //If the token is provided, verify it
        const decodedToken = await admin.auth().verifyIdToken(idToken);

        //Attach the verified user data to the request object
        req.user = decodedToken;
        next();      //Pass the request to the syncController

    }catch (e){

        //If verification fails, block the request and return and error
        console.error("Request Blocked: Invalid or expired token: ", e);
        res.status(403).send({error: 'Invalid or expired token'});
        
    }
}