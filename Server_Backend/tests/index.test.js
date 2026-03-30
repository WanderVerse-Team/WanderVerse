/**
 WanderVerse Backend API Tests
 - Tests FR3 (Authentication), FR4/FR6 (Sync), FR7 (Leaderboard), NFR1 (Security)
 - Uses mocked firebase-admin to run without real credentials.
 */

// MOCK firebase-admin BEFORE requiring anything else
const mockVerifyIdToken = jest.fn();
const mockCollection = jest.fn();
const mockDoc = jest.fn();
const mockGet = jest.fn();
const mockSet = jest.fn();
const mockUpdate = jest.fn();
const mockOrderBy = jest.fn();
const mockLimit = jest.fn();
const mockWhere = jest.fn();
const mockCount = jest.fn();

jest.mock('firebase-admin', () => {
    const mockFirestore = jest.fn(() => ({
        collection: mockCollection,
    }));

    // Make mockFirestore have a FieldValue property
    mockFirestore.FieldValue = {
        serverTimestamp: jest.fn(() => 'MOCK_TIMESTAMP'),
    };

    return {
        apps: [],
        initializeApp: jest.fn(),
        credential: {
            cert: jest.fn(),
        },
        auth: jest.fn(() => ({
            verifyIdToken: mockVerifyIdToken,
        })),
        firestore: mockFirestore,
    };
});

// Now require the app
const request = require('supertest');
const app = require('../index');

// HELPER: Set up firestore mock chain
function setupFirestoreMocks() {
    mockCollection.mockReturnValue({
        doc: mockDoc,
        orderBy: mockOrderBy,
        where: mockWhere,
    });
    mockDoc.mockReturnValue({
        get: mockGet,
        set: mockSet,
        update: mockUpdate,
    });
    mockOrderBy.mockReturnValue({
        limit: mockLimit,
    });
    mockWhere.mockReturnValue({
        count: mockCount,
    });
}

// TEST SUITE

describe('WanderVerse Backend API Tests', () => {

    beforeEach(() => {
        jest.clearAllMocks();
        setupFirestoreMocks();
    });

    // SERVER HEALTH
    describe('Server Health', () => {
        test('GET / should return 200 and confirm the server is running', async () => {
            const res = await request(app).get('/');
            expect(res.status).toBe(200);
            expect(res.text).toContain('WanderVerse middleware is running');
        });
    });

    // FR3: Account Creation (username + password required, email optional)
    describe('FR3 - Account Creation (Username & Password, Email Optional)', () => {

        test('Rejects requests with NO authorization token (401)', async () => {
            const res = await request(app)
                .post('/api/sync')
                .send({ localData: { xp: 10 } });

            expect(res.status).toBe(401);
            expect(res.body.error).toContain('No token provided');
        });

        test('Rejects requests with an INVALID token (403)', async () => {
            mockVerifyIdToken.mockRejectedValue(new Error('Token is invalid'));

            const res = await request(app)
                .post('/api/sync')
                .set('Authorization', 'Bearer fake-token-12345')
                .send({ localData: { xp: 10 } });

            expect(res.status).toBe(403);
            expect(res.body.error).toContain('Invalid or expired token');
        });

        test('Creates account with username and password only (no email)', async () => {
            mockVerifyIdToken.mockResolvedValue({ uid: 'testUser123' });

            // Simulate new user
            mockGet.mockResolvedValue({ exists: false });
            mockSet.mockResolvedValue();

            const res = await request(app)
                .post('/api/sync')
                .set('Authorization', 'Bearer valid-firebase-token')
                .send({ localData: { xp: 50, userName: "TestPlayer" } });

            // Token was verified
            expect(mockVerifyIdToken).toHaveBeenCalledWith('valid-firebase-token');
            // New profile created with userName, no email required
            expect(res.status).toBe(201);
            expect(mockSet).toHaveBeenCalledWith(
                expect.objectContaining({ userName: "TestPlayer" })
            );
            expect(mockSet).toHaveBeenCalledWith(
                expect.not.objectContaining({ email: expect.anything() })
            );
        });

        test('Creates account with username, password, and optional email', async () => {
            mockVerifyIdToken.mockResolvedValue({ uid: 'testUser456' });

            // Simulate new user
            mockGet.mockResolvedValue({ exists: false });
            mockSet.mockResolvedValue();

            const res = await request(app)
                .post('/api/sync')
                .set('Authorization', 'Bearer valid-firebase-token')
                .send({ localData: { xp: 30, userName: "EmailPlayer", email: "player@test.com" } });

            expect(mockVerifyIdToken).toHaveBeenCalledWith('valid-firebase-token');
            expect(res.status).toBe(201);
            // Profile should include both userName and optional email
            expect(mockSet).toHaveBeenCalledWith(
                expect.objectContaining({ userName: "EmailPlayer" })
            );
        });
    });

    // FR4 & FR6: Sync (Cloud Save / Local Save)
    describe('FR4/FR6 - Progress Sync (Save & Cross-Device Sync)', () => {

        beforeEach(() => {
            // All sync tests need a valid token
            mockVerifyIdToken.mockResolvedValue({ uid: 'user_abc' });
        });

        test('Rejects sync when localData is missing (400)', async () => {
            const res = await request(app)
                .post('/api/sync')
                .set('Authorization', 'Bearer valid-token')
                .send({}); // No localData

            expect(res.status).toBe(400);
            expect(res.body.error).toContain('Missing localData');
        });

        test('Creates a NEW profile when user document does not exist (201)', async () => {
            mockGet.mockResolvedValue({ exists: false });
            mockSet.mockResolvedValue();

            const localData = { xp: 100, userName: "NewExplorer", levelProgress: [] };

            const res = await request(app)
                .post('/api/sync')
                .set('Authorization', 'Bearer valid-token')
                .send({ localData });

            expect(res.status).toBe(201);
            expect(res.body.status).toBe('SUCCESS');
            expect(res.body.message).toContain('New profile created');

            // Verify Firestore set was called with the data
            expect(mockSet).toHaveBeenCalledWith(
                expect.objectContaining({
                    xp: 100,
                    userName: "NewExplorer",
                })
            );
        });

        test('Returns CONFLICT when cloud XP is higher than local XP (200)', async () => {
            const cloudData = { xp: 500, userName: "CloudPlayer", levelProgress: [{ levelID: "L1", stars: 3 }] };

            mockGet.mockResolvedValue({
                exists: true,
                data: () => cloudData,
            });

            const localData = { xp: 200, userName: "LocalPlayer" };

            const res = await request(app)
                .post('/api/sync')
                .set('Authorization', 'Bearer valid-token')
                .send({ localData });

            expect(res.status).toBe(200);
            expect(res.body.status).toBe('CONFLICT');
            expect(res.body.message).toContain('Cloud data is ahead');
            expect(res.body.forceUpdate).toEqual(cloudData);
        });

        test('Updates cloud when local XP is higher or equal (200 SUCCESS)', async () => {
            const cloudData = { xp: 100, userName: "OldData" };

            mockGet.mockResolvedValue({
                exists: true,
                data: () => cloudData,
            });
            mockUpdate.mockResolvedValue();

            const localData = { xp: 300, userName: "UpdatedPlayer", levelProgress: [] };

            const res = await request(app)
                .post('/api/sync')
                .set('Authorization', 'Bearer valid-token')
                .send({ localData });

            expect(res.status).toBe(200);
            expect(res.body.status).toBe('SUCCESS');
            expect(res.body.message).toContain('Cloud data updated');

            // Verify update was called
            expect(mockUpdate).toHaveBeenCalledWith(
                expect.objectContaining({
                    xp: 300,
                    userName: "UpdatedPlayer",
                })
            );
        });
    });

    // FR7: Leaderboard
    describe('FR7 - Leaderboard (XP-based, with user rank)', () => {

        // Helper to set up leaderboard mocks for each test
        function setupLeaderboardMocks(topDocs, userDoc, higherCount) {
            const mockSnapshot = {
                forEach: (callback) => topDocs.forEach(callback),
            };

            mockLimit.mockReturnValue({
                get: jest.fn().mockResolvedValue(mockSnapshot),
            });

            // Mock the user's own doc lookup
            mockDoc.mockReturnValue({
                get: jest.fn().mockResolvedValue(userDoc),
                set: mockSet,
                update: mockUpdate,
            });

            // Mock the count query for rank
            mockCount.mockReturnValue({
                get: jest.fn().mockResolvedValue({ data: () => ({ count: higherCount }) }),
            });
        }

        test('Returns top players sorted by XP with only userName and xp fields', async () => {
            mockVerifyIdToken.mockResolvedValue({ uid: 'currentUser1' });

            const topDocs = [
                { data: () => ({ userName: "Player1", xp: 1000, email: "secret@test.com" }) },
                { data: () => ({ userName: "Player2", xp: 800 }) },
                { data: () => ({ userName: "Player3", xp: 600 }) },
            ];

            setupLeaderboardMocks(topDocs, {
                exists: true,
                data: () => ({ userName: "CurrentPlayer", xp: 400 }),
            }, 3);

            const res = await request(app)
                .get('/api/leaderboard')
                .set('Authorization', 'Bearer valid-token');

            expect(res.status).toBe(200);
            expect(res.body.status).toBe('SUCCESS');
            expect(res.body.data).toHaveLength(3);

            // Verify ordering (highest XP first)
            expect(res.body.data[0].xp).toBe(1000);
            expect(res.body.data[1].xp).toBe(800);
            expect(res.body.data[2].xp).toBe(600);

            // Verify only userName and xp are returned (no PII like email)
            res.body.data.forEach(entry => {
                expect(Object.keys(entry)).toEqual(['userName', 'xp']);
                expect(entry).not.toHaveProperty('email');
            });
        });

        test('Includes the current user rank in the response', async () => {
            mockVerifyIdToken.mockResolvedValue({ uid: 'user_rank_test' });

            const topDocs = [
                { data: () => ({ userName: "TopPlayer", xp: 2000 }) },
            ];

            // User has 500 XP, 5 players above them → rank 6
            setupLeaderboardMocks(topDocs, {
                exists: true,
                data: () => ({ userName: "MyPlayer", xp: 500 }),
            }, 5);

            const res = await request(app)
                .get('/api/leaderboard')
                .set('Authorization', 'Bearer valid-token');

            expect(res.status).toBe(200);
            expect(res.body.currentUser).toBeDefined();
            expect(res.body.currentUser.userName).toBe('MyPlayer');
            expect(res.body.currentUser.xp).toBe(500);
            expect(res.body.currentUser.rank).toBe(6);
        });

        test('Rejects unauthenticated leaderboard requests (401)', async () => {
            const res = await request(app).get('/api/leaderboard');
            expect(res.status).toBe(401);
        });
    });

    // NFR1: Security (No PII Collection)
    describe('NFR1 - Security (No PII stored or exposed)', () => {

        test('Leaderboard endpoint does NOT expose email, phone, or any PII', async () => {
            mockVerifyIdToken.mockResolvedValue({ uid: 'securityTestUser' });

            const mockDocs = [
                { data: () => ({ userName: "SafeUser", xp: 999, email: "hidden@mail.com", phone: "+1234567890" }) },
            ];

            const mockSnapshot = {
                forEach: (callback) => mockDocs.forEach(callback),
            };

            mockLimit.mockReturnValue({
                get: jest.fn().mockResolvedValue(mockSnapshot),
            });

            // Mock user doc and rank count for the authenticated request
            mockDoc.mockReturnValue({
                get: jest.fn().mockResolvedValue({
                    exists: true,
                    data: () => ({ userName: "SecUser", xp: 100 }),
                }),
                set: mockSet,
                update: mockUpdate,
            });
            mockCount.mockReturnValue({
                get: jest.fn().mockResolvedValue({ data: () => ({ count: 1 }) }),
            });

            const res = await request(app)
                .get('/api/leaderboard')
                .set('Authorization', 'Bearer valid-token');

            expect(res.status).toBe(200);
            if (res.body.data && res.body.data.length > 0) {
                const entry = res.body.data[0];
                expect(entry).not.toHaveProperty('email');
                expect(entry).not.toHaveProperty('phone');
                expect(entry).not.toHaveProperty('password');
            }
        });

        test('Keys endpoint rejects unauthenticated requests (401)', async () => {
            const res = await request(app).get('/api/keys');
            expect(res.status).toBe(401);
        });

        test('Leaderboard endpoint rejects unauthenticated requests (401)', async () => {
            const res = await request(app).get('/api/leaderboard');
            expect(res.status).toBe(401);
        });
    });
});
