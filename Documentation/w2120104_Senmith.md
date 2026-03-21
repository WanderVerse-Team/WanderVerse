# w2120104_Senmith

0
Informatics Institute of Technology
In collaboration with
University of Westminster
Software Development Group Project
5COSC021C.Y
Module Leader : Mr. Banuka Athuraliya
w2120104_20240756_Senmith - CS94_CW 1 Individual
Loku Kankanamge Senmith Thenura Sahajeewa

-- 1 of 57 --

WanderVerse CS-94 ⅱ
Declaration Page
I hereby certify that this project report, as well as all elements associated with it, is my own work
and none of this has been submitted before for any other academic program nor is in the progress
of being submitted.
Full Name: Loku Kankanamge Senmith Thenura Sahajeewa
Registration Number: 20240756
UOW Number: w2120104

-- 2 of 57 --

WanderVerse CS-94 ⅲ
Abstract + Keyword
WanderVerse is a gamified educational app that is targeting the local school curriculum. The
following is the individual report that highlights the important planning and technological phases
involved for the development of the system.
The project begins with System Requirement Specification, wherein it identifies stakeholders
using the Onion model. Moreover, this stage determines functional and non functional
requirements for an ideal learning system. A major focus of this stage weighs ethical issues related
to creating applications for minors, described in Social, Legal, Ethical, Professional. SLEP
considered data protection regulations, as well as minimizing risks associated with screen time
monetization.
In addition, the project also includes a definition of System Architecture Design. A 3 tier layered
architecture is suggested, which uses unity client with Local JSON support for offline
functionality, together with a Node.js layer supporting secure synchronization between the cloud
database. System design is also described in a series of Class diagrams, Sequence diagrams and
user interface designs. This allows a full system design to be developed, which will guide the
implementation of WanderVerse.
Keywords: Gamification, 3 Tier Layered Architecture, System Requirement Specification.

-- 3 of 57 --

WanderVerse CS-94 ⅳ
Acknowledgement
I would like to extend my deepest appreciation to the SDGP lecture panel for structuring this
comprehensive module and providing the resources and framework necessary for such a project to
be possible. Their guidance throughout the semester has been instrumental in defining the scope
and direction of “WanderVerse”.
I am deeply grateful to my project supervisor, Mrs. Kasuni Welihinda, for the continued
mentorship extended to us during this study. Indeed, her comments and attention to detail were an
immense help in molding the quality of this report to meet high standards academically.
I would also like to thank Mr.Manul Singhe, who provided much needed support as well as insights
during the development phases and thus was of great help in overcoming several technical
difficulties.
I am also indebted to the panel of lecturers at the Informatics Institute of Technology (IIT) for their
dedication to teaching and equipping me with the theoretical knowledge to undertake a software
development project of this nature.
Last but not least, special thanks go to my teammates. This work is the proof of our collective
effort, resilience and mutual support during this journey.

-- 4 of 57 --

WanderVerse CS-94 ⅴ
Table of Contents
Declaration Page ⅱ
Abstract + Keyword ⅲ
Acknowledgement ⅳ
Table of Contents ⅴ
List of Figures ⅵ
List of Tables ⅷ
Abbreviations Table ⅸ
Chapter 4: System Requirement Specifications (SRS) 1
4.1 Chapter Overview 1
4.2 Stakeholder Analysis 1
4.2.1 Onion Model 1
4.2.2 Stakeholder Description 2
4.3 Selection of requirement elicitation techniques/ methods 4
4.3.1 Document Analysis 4
4.3.2 Brainstorming 4
4.3.3 Market Research 4
4.3.4 Questionnaires 4
4.3.5 Expert and Academic Validation 5
4.4 Discussion and Analysis of Results 5
4.4.1 Insights from Document Analysis 5
4.4.2 Results from brainstorming 5
4.4.3 Findings from Market Research 6
4.4.4 Analysis of Questionnaires 6
4.5 Use Case Diagram 11
4.6 Use Case Description 12
4.6.1 Purchase Streak Freezes 12
4.6.2 Purchase Avatar Skin 14
4.7 Functional Requirements 15
4.8 Non Functional Requirements 17
4.9 Chapter Summary 18
Chapter 5: Social, Legal, Ethical and Professional Issues 19
5.1 Chapter Overview 19
5.2 Dataset Ethical Clearance 19
5.3 SLEP Issues and Mitigation 19
5.4 Chapter Summary 20

-- 5 of 57 --

WanderVerse CS-94 ⅵ
Chapter 6: System Architecture & Design 21
6.1 Chapter Overview 21
6.2 System Architecture Design 22
6.2.1 Tier 1: Presentation Layer (Client) 22
6.2.2 Tier 2: Application Layer (MiddleWare) 24
6.2.3 Tier 3: Data Layer 25
6.3 System Design 26
6.3.1 Class Diagram 26
6.3.2 Sequence Diagram 27
6.3.3 UI design and Mockups 30
6.3.4 Activity Diagram 32
6.4 Chapter Summary 35
Appendix 36

-- 6 of 57 --

WanderVerse CS-94 ⅶ
List of Figures
Figure 4.2.1 - Onion model 1
Table 4.2.2 - Stakeholder Description Table 3
Figure 4.4.4.1 Daily screen time among respondents or their children 7
Figure 4.4.4.2 - Most observed negative effects of excessive screen time 8
Figure 4.4.4.3 - Respondents opinion on gamification replacing addictive gaming 9
Figure 4.4.4.4 - Requested feature chart 10
Figure 4.5 - Use Case Diagram 11
Table 4.6.1 - Use Case Description 1 13
Table 4.6.2 - Use Case Description 2 15
Table 4.7 - Functional Requirements Table 16
Table 4.8 - Non Functional Requirements 18
Figure 6.2 - System Architecture Design 21
Figure 6.2.1 - Presentation Layer 22
Figure 6.2.2 - Application Layer (Middleware) 23
Figure 6.2.3 - Data Layer 24
Figure 6.3.1 - Class diagram 25
Figure 6.3.2.1 - Sequence Diagram 1 26
Figure 6.3.2.2 - Sequence Diagram 2 27
Figure 6.3.2.3 - Sequence Diagram 3 28
Figure 6.3.3.1 - Splash screen, Sign In and Sign Up 29
Figure 6.3.3.2 - Selecting Grade, Subject and Language 29
Figure 6.3.3.3 Dashboard, LeaderBoard and World maps 30
Figure 6.3.4.1 - Background Data Synchronization 31
Figure 6.3.4.2 - Initial Profile Recovery 32
Figure 6.3.4.3 - Leaderboard Caching Strategy 33

-- 7 of 57 --

WanderVerse CS-94 ⅷ
List of Tables
Table 4.2.2 - Stakeholder Description Table 3
Table 4.6.1 - Use Case Description 1 13
Table 4.6.2 - Use Case Description 2 15
Table 4.7 - Functional Requirements Table 16
Table 4.8 - Non Functional Requirements 18

-- 8 of 57 --

WanderVerse CS-94 ⅸ

-- 9 of 57 --

WanderVerse CS-94 ⅸ
Abbreviations Table
Abbreviation Full Term
IIT Informatics Institute of Technology
UOW University of Westminster
SDGP Software Development Group Project
CW Coursework
SRS System Requirement Specification
SLEP Social, Legal, Ethical, Professional
UC Use Case
FR Functional Requirements
NFR Non Functional Requirements
API Application Programming Interface
UI User Interface
JSON JavaScript Object Notation
IAP In App Purchases
OS Operating System
SDK Software Development Kit
XP Experience Points
COPPA Children’s Online Privacy Protection Act
BCS British Computer Society
NoSQL Not Only SQL

-- 10 of 57 --

WanderVerse CS-94 1
Chapter 4: System Requirement Specifications (SRS)
4.1 Chapter Overview
This chapter describes the development of WanderVerse by formally defining the system’s
specifications. The chapter opens with a stakeholder analysis to identify the primary groups
influencing the project, followed by an explanation of the elicitation methods chosen to gather
their input. These findings are then critically analyzed to highlight the key problems and user
needs. The Chapter concludes by converting these insights into functional and non functional
requirements, which provide the technical criteria for the implementation phase.
4.2 Stakeholder Analysis
4.2.1 Onion Model
Figure 4.2.1 - Onion model

-- 11 of 57 --

WanderVerse CS-94 2
4.2.2 Stakeholder Description
Stakeholder Viewpoint
Functional beneficiary
Students Students who are the direct end users, require the app to be
entertaining and easy to use. The mini games should be fun and
the interface must be interactive enough to catch and keep their
attention.
Financial beneficiary
Parents Parents have an economic viewpoint here. They see the app as a
cost-effective educational resource. By using the app, they may
save money spent on tuition and other supplementary materials.
Development Team In the future, the development team views the app as a potential
source of revenue, through sales or ads or as a boost for career
advancement.
Social beneficiary
The Educational
Community
Schools along with the wider educational community sees this as
a way to modernize education and make learning effective and
appealing. The educational community benefits from having a
student population that is more engaged with studies.
Operational beneficiary
Teachers Teachers see this app as a supplementary tool, which helps them
in making the students understand their syllabus concepts more

-- 12 of 57 --

WanderVerse CS-94 3
effectively. If students practice the topics at home, it makes the
teacher’s job easier the following day.
Negative Stakeholders
Competitors Developers with similar apps see this app as a market threat that
might reduce their own user base
Hackers Hackers see this app as a potential target to exploit for data theft
or service disruption.
Tuition Teachers Tuition teachers see this app as a potential threat for their
customer base
Regulatory
Government The government monitors the app to see whether the content
complies with the syllabus if schools were to recommend it to
students.
PlayStore/ App Store Platform regulators see this app as a subject that must follow
compliance with privacy and safety guidelines.
Experts
Teachers The teachers look at this app so see if the mini-games actually
teach the students the correct concepts.
Supervisor They focus on the technical quality of the app and ensure the team
follows proper software development practices.
Neighboring systems

-- 13 of 57 --

WanderVerse CS-94 4
Mobile OS The mobile ecosystem is a neighboring system for the app. The
app must adhere to the technical constraints and specifications of
the ecosystem.
Table 4.2.2 - Stakeholder Description Table
4.3 Selection of requirement elicitation techniques/ methods
To make sure the system requirements were practical, the data gathering process was approached
from several directions by combining primary research and feedback from potential users,
stakeholders and experts. With this the requirements for WandeVerse were built based on real-
world data rather than just theory.
4.3.1 Document Analysis
Since the pilot project is based on the Sri Lankan grade 3 mathematics syllabus, the starting point
was the official curriculum. The grade 3 mathematics textbook was thoroughly analysed along
with the corresponding teacher’s guide. This was a necessary step to guarantee that every feature
and question in game aligns with what the students are actually learning in the classroom.
4.3.2 Brainstorming
Regular group discussions were the driving force behind the project's creativity. These sessions
were used to navigate both technical hurdles, like selecting the right development tools, and design
challenges. The most critical task during these meetings was the gamification process. Figuring
out how to transform raw theory into interactive gameplays. These collaborative brainstorm
sessions allowed the team to generate innovative and creative concepts that kept the learning
objectives clear while providing the ultimate game experience for the users.
4.3.3 Market Research
The current market landscape was examined by testing popular applications like Duolingo,
SplashLearn and ExamHub. Rather than just imitating their features, the gaps in their service
models were analysed to find weaknesses and gain a competitive edge.

-- 14 of 57 --

WanderVerse CS-94 5
4.3.4 Questionnaires
To understand the ecosystem the app would enter, data from a wide demographic, ranging from
primary students to teachers and parents and other stakeholders were gathered. The questionnaires
were designed to map out the digital habits of users and to uncover parental concerns regarding
screen time. This feedback was crucial for positioning the app as a productive educational tool
rather than just a digital distraction.
4.3.5 Expert and Academic Validation
To avoid developing the system in isolation, a theoretical framework based on existing research
into child psychology and game based learning was built. This helped in improving the
understanding on how to maintain engagement without overwhelming young students. To back up
this research, university lecturers who specialized in game design and pedagogy were consulted to
refine our approach in balancing fun and learning.
4.4 Discussion and Analysis of Results
After conducting the data gathering methods described above, the findings were analyzed to define
the core features of WanderVerse. The insights from each method directly influenced the final
design of the application.
4.4.1 Insights from Document Analysis
The deep dive into the syllabus revealed that the order of teaching is just as important as the
learning outcomes itself. It was also found that the concepts in the textbooks are built upon one
another. For example, a student must master simple addition before moving onto money related
problems. As a result, the game levels were structured to strictly follow the textbook’s progression
in a sequential manner. Furthermore exact terminologies found in the government textbooks will
be used in the application to avoid confusing students who are used to classroom language.
4.4.2 Results from brainstorming
The brainstorming sessions allowed the team to filter out ideas based on technical feasibility.
Initially there were complex ideas for 3D open worlds, but after further discussions it was decided

-- 15 of 57 --

WanderVerse CS-94 6
that it would be too heavy for the average mid range smartphone used by our target demographic.
Then the design was shifted to a lightweight 2D aesthetic that is visually appealing but also can be
run on the average mid range smartphone.
4.4.3 Findings from Market Research
Analyzing competitors like Duolingo and SplashLearn clarified the unique selling proposition of
WanderVerse. While the global apps are visually stunning they fail in the local context. They
often reference foreign currency or examples irrelevant to a Sri Lankan child. Furthermore they
require constant internet connection. Hence the offline-first requirement became a high priority
and the app is designed to be downloaded once and played without data.
And the other local competitors lack proper story telling and visual appeal. It was also noticeable
that local apps are not as alluring to the audience and highly leaned towards learning and less fun.
As a result WanderVerse was designed to seamlessly integrate education, allowing users to learn
without realizing it, making the experience feel like casual gaming.
4.4.4 Analysis of Questionnaires
To validate assumptions and the potential for gamified learning in Sri Lanka, a survey was
conducted using google forms. The questionnaire gathered data from a diverse sample of 169
respondents (170 - 1 no consent), consisting of 53 parents, 40 school students, 9 teachers and a
mix of university students and professionals. This demographic mix ensured that the gathered
requirements reflected the needs of all key stakeholders in the onion model, from direct
beneficiaries (students) to decision makers (parents and teachers).
Digital Habits and the Problem - The survey results strongly validate the core problem: children
are spending a significant amount of time on screens. The data revealed that 23.5% of respondents
(or their children) spend 1-2 hours on gaming or social media daily, while 26.5% spend 2-4 hours
and 17.6% spend between 4-6 hours. Here the percentages are based on the total number of
responses (102 as some didn't answer), rather than looking at each group separately.

-- 16 of 57 --

WanderVerse CS-94 7
When asked about the negative effects of this excessive screen time, the results were clear. The
most cited issue was “Reduced attention span” (73 responses) and “Reduced outdoor activity” (72
responses) and “Poor academic performance” (55 responses). These statistics confirm that the
traditional education system is struggling to compete with the high stimulation environment of
digital entertainment, leading to a decline in academic focus.
Teachers
Parents

-- 17 of 57 --

WanderVerse CS-94 8
Students

-- 18 of 57 --

WanderVerse CS-94 9
Figure 4.4.4.1 Daily screen time among respondents or their children

-- 19 of 57 --

WanderVerse CS-94 10
Teachers
Parents
Students
Figure 4.4.4.2 - Most observed negative effects of excessive screen time

-- 20 of 57 --

WanderVerse CS-94 11
Validation of Gamification as a Solution - The survey also depicts the understanding of
stakeholders viewing gamification as a viable solution. The response was highly positive. A
combined 78% of respondents either agreed or strongly agreed that gamification could motivate
children to learn better than traditional methods. Furthermore, when asked if an educational game
could effectively replace a portion of the time currently spent on addictive, non educational games,
75% responded positively.
This indicates strong market readiness for WanderVerse. Parents and teachers are not looking to
ban screens entirely, which would be very difficult, but are eager for a “Trojan horse” solution that
turns that screen time into productive learning time.
Figure 4.4.4.3 - Respondents opinion on gamification replacing addictive gaming
Feature prioritization - The questionnaire was used to prioritize the functional requirements of
the app. Respondents were asked to select which features they believed were most important for
the app. The results directly influenced the project scope.
● Syllabus aligned lessons (121 votes) was the top requested feature, confirming that the app
must strictly adhere to the local curriculum to be valued by parents and students.
● Age appropriateness UI (111 votes) highlighted the need for simple , child friendly design

-- 21 of 57 --

WanderVerse CS-94 12
● Safe, ad-free experience (106 votes) showed a critical ethical requirement for the app which
is targeting minors.
● Rewards, badges and progress tracking (103 votes) validated the need for a robust
gamification process.
● Offline access (94 votes) was identified as an essential feature by a significant portion of
the respondents, reinforcing the decision to build an offline first architecture to
accommodate the local infrastructure.
Figure 4.4.4.4 - Requested feature chart

-- 22 of 57 --

WanderVerse CS-94 13

-- 23 of 57 --

WanderVerse CS-94 14
4.5 Use Case Diagram

-- 24 of 57 --

WanderVerse CS-94 15
Figure 4.5 - Use Case Diagram

-- 25 of 57 --

WanderVerse CS-94 16
4.6 Use Case Description
4.6.1 Purchase Streak Freezes
Use Case Name Purchase streak freeze bundle
Use Case ID UC-001
Description The user purchases a bundle containing “2 Streak Freezes” to
protect their daily login streak from breaking if they miss a day.
Priority High (Monetization)
Primary Actor User
Supporting Actors Google Play Store
Pre-Conditions ● User must be logged in to a verified account
● Device must have an internet connection
● User must be in the store section of the UI
Trigger The user taps the “Buy 2 streak freezes” button
Main flow Actors System
● User select the bundle
and taps buy
● User answer the
security question
● User confirms
payment details on
google play
● System triggers the
parental fate to verify age
● System initiates the
transaction request via
Unity IAP to google play
● System receives the
purchase receipt, sends it to

-- 26 of 57 --

WanderVerse CS-94 17
validation and add items to
the inventory upon success
Exception flow Actors System
● Parental gate security
question is answered
incorrectly
● User cancels the
payment on google
play overlay
● The system denies the
purchase attempt and
displays an “incorrect
answer” message.
Alternate flow Actors System
● If the validation response is
delayed, the system queues
the receipt locally and
retries the validation
automatically when
connection is stable.
Exclusions Subscriptions
Post Conditions The user’s inventory count for “Streak Freezes” increases by 2.
The transaction is logged in the database.
Table 4.6.1 - Use Case Description 1

-- 27 of 57 --

WanderVerse CS-94 18
4.6.2 Purchase Avatar Skin
Use Case Name Purchase Avatar Skin
Use Case ID UC-002
Description The user purchases a permanent visual customization for their in
game mascot from real currency
Priority Medium (cosmetic)
Primary Actor User
Supporting Actors Google Play Store
Pre-Conditions ● The selected skin must be currently locked
● User must be logged in
Trigger User selects a locked in the shop inside the menu
Main flow Actors System
● User selects a locked
skin
● User taps buy button
● User answers security
question
● User confirms the
payment details on
google play store
● System displays the price
and the model of the skin
● System presents the
security question
● System request the
payment receipt and
validates it via the backend
● Unlocks the skin
Exception flow Actors System

-- 28 of 57 --

WanderVerse CS-94 19
● System detects the item is
already owned and restores
it for free
Alternate flow Actors System
Exclusions Purchasing skins using in-game currency
Post Conditions Specific skin ID is permanently added to the user profile and the
skin appears as owned in the UI.
Table 4.6.2 - Use Case Description 2
4.7 Functional Requirements
Requirements list Priority
Level
Description
FR1 Sign in & User Profile
creation
Critical The app must allow students to sign
in and create their own unique user
profile
FR2 Core Gameplay mechanics Critical The app must allow the users to
interact with the elements in the
game
FR3 Core Content Critical The app must cover all the relevant
content in the syllabus along with the
learning outcomes.

-- 29 of 57 --

WanderVerse CS-94 20
FR4 Curriculum Selection Critical The app must allow the users to
select the relevant grade and subject
FR5 Feedback Critical The app must give instant feedback
depending on whether the answers
are correct or wrong.
FR5 User Dashboard Desirable The app must have a user dashboard
for each user showing their details
and progression.
FR6 Voice Recognition Desirable A voice recognition feature for
students to learn pronunciation
FR7 Progression System Desirable Centralized system for user
progression to unlock next level only
after certain requirements are
satisfied
FR8 Sound Effects Luxury Certain sound effects for different
interactions and environments.
FR8 Mascot Integration Luxury Integrate the mascot into the system
to help and guide students across the
game
FR9 User Statistics Luxury Statistics of user to get insights on
their progression, weak areas and
strong areas

-- 30 of 57 --

WanderVerse CS-94 21
FR10 Dark Mode Luxury A dark mode and an option to switch
between light and dark mode for eye
comfort of the users

-- 31 of 57 --

WanderVerse CS-94 22
Table 4.7 - Functional Requirements Table

-- 32 of 57 --

WanderVerse CS-94 23
4.8 Non Functional Requirements
Requirements list Description
NFR 1 Response Time The loading time of the main application and its mini-
games should be within 3 minutes to maintain student
engagement.
NFR 2 Resource Consumption The application along with the games should be light
weight to run efficiently on mid range smartphones
consuming resources as less as possible
NFR 3 Offline Availability The application must be fully functional without an
active internet connection
NFR 4 Data saves The game must save progress safely to make sure that
existing save files won’t get corrupted if the save
process gets terminated midway. (Atomic saves)
NFR 5 Synchronization The application must automatically sync local
progress with the cloud when connected to the
internet.
NFR 6 Audience Appropriateness The UI and terminology must be suitable for the
target user group and be easy to navigate
NFR 7 Authentication Security User authentication credentials must be securely
managed to prevent unauthorized access

-- 33 of 57 --

WanderVerse CS-94 24
NFR 8 Screen Adaptability The UI must be responsive and adapt to various
screen sizes and aspect ratios without cutting of
essential elements
Table 4.8 - Non Functional Requirements
4.9 Chapter Summary
In this chapter, the system requirement specification for WanderVerse has been defined. A
stakeholder analysis was initiated for identifying major beneficiaries. Functional and non
functional requirements for WanderVerse were compiled from user requirements. The chapter has
ended with Use Cases that defined major interactions such as purchase of premium features and
parental verification.

-- 34 of 57 --

WanderVerse CS-94 25
Chapter 5: Social, Legal, Ethical and Professional Issues
5.1 Chapter Overview
This chapter looks at the wider impact of the WanderVerse application, covering the social, legal
and professional responsibilities that come up with building software for young children. The
section explains how the project handles data privacy, follows the rules set for educational content
and sticks to professional standards like the BCS code of conduct.
5.2 Dataset Ethical Clearance
WanderVerse does not involve gathering personal information from real people for research, nor
does it use any datasets.
5.3 SLEP Issues and Mitigation
Building an app for children comes with a lot of responsibility, and there are several important
issues to consider. A major social issue is the “Digital Divide.” Not every student in Sri Lanka
has access to the latest smartphones nor has a stable internet connection. If the app requires high
end specifications or constant internet connection, it would leave many students behind.
WanderVerse is designed to run smoothly on mid range phones without any performance issues.
More importantly, the app uses an offline first approach, ensuring that users can play games and
save their progress locally without needing an internet connection. This enables the children in
rural areas or those with limited data to still use the app for learning.
From a legal standpoint, protecting the privacy of minors is the biggest priority. Laws like COPPA
and local data protection regulations are very strict about collecting data from children.
WanderVerse handles this by using a system that is private by default. The app does not ask or
store a user’s real name, address, school or phone number. Instead it assigns a unique User ID to
keep track of progress. By not collecting any sensitive information in the first place, the application
stays compliant with the law and keeps the user’s identities safe from potential data breaches.

-- 35 of 57 --

WanderVerse CS-94 26
On the ethical side, making money from an educational app for kids is a sensitive topic. Including
In-App purchases for things like skins creates a risk that a child might accidentally spend money
without the parent’s knowledge. To handle this ethically, the system includes a Parental Gate. This
is a security feature that forces a user to complete a specific action that only an adult would
understand before a purchase can go through. This ensures that the app can be sustainable
financially without taking advantage of its young users.
Finally regarding the professional issues, the development process follows the standards set by the
BCS code of conduct. This means the team has a duty to work competently and deliver a high
quality product. This is achieved by using standard industry tools like Unity and GitHub to manage
the code properly. The team also uses the Agile methodology to constantly test and improve the
software. By performing stress tests to ensure data does not get lost when playing offline, the
developers are fulfilling their professional obligation to provide reliable and working tools for the
users.
5.4 Chapter Summary
To summarize, this chapter discussed the non technical responsibilities of the WanderVerse
project. It is clarified that no datasets were used in making the app and how the app addresses
social gaps through offline access is also explained. Furthermore legal rights are protected by
keeping user data anonymous while ethical risks regarding money are managed with parental
controls.

-- 36 of 57 --

WanderVerse CS-94 27
Chapter 6: System Architecture & Design
6.1 Chapter Overview
This chapter is the technical specification for the WanderVerse app and fills the gap between the
system requirements specified in the SRS and the final software deliverable. This chapter begins
with the definition of the System Architecture Design and discusses the need for a 3 Tier Layered
Architecture. This chapter clarifies how the WanderVerse system combines the Unity client with
the Node.js server and the firebase cloud support in order to address the offline first functionality
and security of the data.
As per the architectural framework, the chapter unfolds the System Design through the detailed
use of UML diagrams. Class Diagrams are utilized in defining the modular, object oriented design
of the key components of the system, including the Game Manager and the Local Data Manager.
Next, the sequence diagrams help in the depiction of the time flow and the interaction sequence of
the client and the server for the complex operations of data synchronization and conflict resolution.
Moreover, the Activity Diagrams are employed in the mapping of the decision related logic of
background processing. Lastly, the chapter descends into the UI design and mock ups, depicting
how the aforementioned technical details are grouped in the high fidelity, child friendly
environment, in tune with the project’s education aspects.

-- 37 of 57 --

WanderVerse CS-94 28
6.2 System Architecture Design
Figure 6.2 - System Architecture Design

-- 38 of 57 --

WanderVerse CS-94 29
6.2.1 Tier 1: Presentation Layer (Client)
The presentation layer revolves around the custom built Android application using the Unity Game
Engine that acts as the principal point of interface for interacting with the students. One of its major
distinguishing features being its strong Local Persistence Manager, whereby it overcomes the
constraint of having to be internet-connected for carrying out major gameplay functionalities.
Rather, the application has an ‘Atomic Save’ system where the system stores its entire gamified
data, user profile and inventory in JSON files directly within the local storage of the device, thus
providing an instantaneous commitment of data with foolproof complete functionality even when
disconnected from the internet, and essential factor being considered for the target group of
WanderVerse.
Handling the communication interface between the internal system and the outside world is the
responsibility of the dedicated Network Manager, running in the background hidden from the UI.
It is constantly checking the internet connectivity of the device and will proceed with the
synchronization of the middleware only when the internet is up and running. The current
implementation of the application helps in optimizing the network usage, not disturbing the smooth
running of the game mechanics and at the same time backing up data into the cloud.

-- 39 of 57 --

WanderVerse CS-94 30
Figure 6.2.1 - Presentation Layer

-- 40 of 57 --

WanderVerse CS-94 31
6.2.2 Tier 2: Application Layer (MiddleWare)
The application layer is the secure brain of the system, built as a custom RESTful API with
Node.js/Express, running on a serverless Vercel platform. This layer acts as a security firewall that
mediates between the database and the client application. The system design choice to have all
communication flow through it as an intermediate layer enforces the principle that the Unity client
will never have write access to the database. This is important in the design to prevent cheating as
strict validation of incoming data allows the server to filter anomalies, like unreasonable
completion times and altered game results, before they can influence the world record.
In addition to the security aspects, this level implements the complex business logic necessary for
data consistency over multiple sessions. This is accomplished by the middleware, which
implements the complex logic of synchronizing data by comparing the timestamps and progress
information of the local JSON cache provided by the application with the information that is stored
on the cloud. This is necessary if a conflict emerges, by which it is meant that a user may play on
multiple devices. This is resolved by the conflict Resolution logic used by the middleware.

-- 41 of 57 --

WanderVerse CS-94 32
Figure 6.2.2 - Application Layer (Middleware)

-- 42 of 57 --

WanderVerse CS-94 33
6.2.3 Tier 3: Data Layer
The data layer relies on the Google Firebase Ecosystem to implement the data layer for the
WanderVerse application. Cloud Firestore is utilized as the main NoSQL database for storing
leaderboards, user profiles and full inventory data. The document based design in Firestore
satisfies the requirement for flexibility and enables horizontal scaling, which is not possible with
traditional relational databases that strictly adhere to schema designs for scaling purposes. When
catering to an increasing number of users from a pilot pool to a mass audience, scalability is
implemented on the backend for high read and write traffic without requiring any major
architecture redesigns.
Besides the role of data storage, this layer also takes care of identity and access management via
firebase authentication. The system uses a safe approach wherein local system created user IDs are
securely mapped to hashed credentials in the cloud to handle so called. In this way, the system
secures user identities via encryption schemes. The master data centralized in the cloud also
secures an easy backup solution for users to readily recover their progress if switching to a new
device or re-installing the application, while the isolation of this information in the cloud keeps it
away from any possible manipulation in the client layer for security reasons.
Figure 6.2.3 - Data Layer

-- 43 of 57 --

WanderVerse CS-94 34
6.3 System Design
6.3.1 Class Diagram
Figure 6.3.1 - Class diagram
The class diagram outlines the modular architecture of WanderVerse. The GameManager controls
the game flow from the center, interacting with the BaseLevelController to manage specific
gameplay mechanics. Data is saved in two ways, with the local manager for offline storage and
SyncManager for Cloud sync and conflict resolution. Additionally, the player class tracks
progression statistics, while the StoreManager handles in-app purchases.

-- 44 of 57 --

WanderVerse CS-94 35
6.3.2 Sequence Diagram
Figure 6.3.2.1 - Sequence Diagram 1
Figure 6.3.2.1 shows the standard background syncing process. When an internet connection is
found or the player completes a specific level the system manager triggers an auto sync. The sync
manager checks for the internet connection and if a connection is found, the sync manager calls
the network client to initiate a POST request with the locally saved JSON payload.
The network client sends the data to the Express.js custom backend API, which validates the data
to prevent cheating and unwanted tampering with the database. Once approved the data is merged
into the Firestore Database by the API. Finally the database confirms the update and returns a
success message, allowing the Sync Manager to mark the local files as ‘Synced’.

-- 45 of 57 --

WanderVerse CS-94 36
Figure 6.3.2.2 - Sequence Diagram 2
The figure 6.3.2.2 shows a server side conflict resolution scenario when the progress in the cloud
database is higher than the progress in the local data. This kind of scenario happens when a user
plays the game on two devices using the same account. When the local data is being sent to the
backend API, it validates the data. The backend API now has the sent data from the system and
the fetched data from the database.
The backend API then sends both these values to the helper logic called the conflict resolver, where
the logic determines which data is most up to date. In this case since the database is ahead, the
conflict resolver takes the decision to keep the progress which is in the cloud database. Then the
backend API sends a success message to the sync manager along with a force update to the local
progress data. The client then updates itself with the up to date data. Now the device is in sync
with the cloud and other devices.

-- 46 of 57 --

WanderVerse CS-94 37
Figure 6.3.2.3 - Sequence Diagram 3
The figure 6.3.2.3 depicts the system’s tolerance regarding the network instability. These kinds of
scenarios happen frequently due to timeouts or packet loss. Instead of completely backing off when
an error occurs, the sync manager uses a retry scheduler to overcome the issue.
After the Sync manager sends the request for a POST to the network client and the network client
returns a server error message, the sync manager triggers the retry scheduler. The retry scheduler
starts a background timer and triggers a sync again after the timer goes off. This trigger makes the
sync manager send a request for a POST to the network client again. This cycle goes on, increasing
the wait time each turn until the sync is successful or a specific number of cycles have been
completed.

-- 47 of 57 --

WanderVerse CS-94 38
6.3.3 UI design and Mockups
Splash Screen, Sign In and Sign Up
Figure 6.3.3.1 - Splash screen, Sign In and Sign Up
Selecting Grade, Subject and Language

-- 48 of 57 --

WanderVerse CS-94 39
Figure 6.3.3.2 - Selecting Grade, Subject and Language

-- 49 of 57 --

WanderVerse CS-94 40
Dashboard / Leaderboard and World maps

-- 50 of 57 --

WanderVerse CS-94 41
Figure 6.3.3.3 Dashboard, LeaderBoard and World maps

-- 51 of 57 --

WanderVerse CS-94 42
6.3.4 Activity Diagram
Figure 6.3.4.1 - Background Data Synchronization
The diagram 6.3.4.1 shows the decision making process behind the application’s background
synchronization feature. The process kicks off automatically as soon as a significant event occurs,
like completing a level. The first logical step is checking whether an internet connection is
available. If the device is offline, the data is queued to be sent later.If an internet connection is
available, the backend API just doesn’t blindly accept the data.

-- 52 of 57 --

WanderVerse CS-94 43
First a validator checks the data to ensure the data is proper to prevent cheating or tampering with
the cloud database. If the data is proper, it enters the conflict resolution phase where the system
decides which save file is ahead (local or the cloud database). If the server sees that the cloud
version has more progress (due to multiple device compatibility), it rejects the local update and
forces the app to download the cloud data. This server side decision making model prevents
accidental data loss and keeps user progression consistent across all devices.
Figure 6.3.4.2 - Initial Profile Recovery

-- 53 of 57 --

WanderVerse CS-94 44
The above diagram 6.3.4.2 visualizes the onboarding flow, specifically what happens when a user
logs into the application on a brand new device. The flow starts with authentication and once the
user proves who they are, the app asks the server if the person has a history. Then the logic is split
into two parts here.
If the database finds an existing profile, the app enters a restoration state, downloading the JSON
save file and overwriting the local empty slots. This allows the user to pick up where they left off
without manually transferring the files. However if the database does not have a matching account,
the system identifies the user as a new user and triggers the initialization logic, setting up a fresh
level 1 profile. This automated processing makes the app easy to install and set up on multiple
devices.

-- 54 of 57 --

WanderVerse CS-94 45
Figure 6.3.4.3 - Leaderboard Caching Strategy

-- 55 of 57 --

WanderVerse CS-94 46
This diagram 6.3.4.3 focuses on performance and efficiency. Downloading the leaderboard list
every time a user opens the leaderboard would waste data and drain the battery. To solve this, the
diagram shows a cache first logic
When the user opens the leaderboard, the system checks for the local timestamp. If the stored data
is newer than 5 minutes, the application skips the internet entirely and loads the list from the
device’s memory. This makes the UI feel instant and smooth. The network path is taken only if
the cache is expired. Also if the network request fails, the system shows the last available data
rather than an error screen. This prioritizes a smooth user experience.
6.4 Chapter Summary
This chapter has described the framework of the system’s architecture and design. The 3-tier
layered architecture was selected to enable offline capability, with local JSON storage and a
Node.js middleware for synchronization purposes, all of which ensured secure synchronization.
Class diagrams and sequence diagrams were used to show the system’s internal working in terms
of logic and objects, respectively. Last, the UI design chapter has shown how these technical
specifications are incorporated into a child-friendly User interface.

-- 56 of 57 --

WanderVerse CS-94 47
Appendix
Appendix A: Website - www.wanderverse.wuaze.com

-- 57 of 57 --