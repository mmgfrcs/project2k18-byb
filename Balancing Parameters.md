# Build Your Business - Balancing Parameters
*This document is updated for version 0.1.24b. Game version number can be seen in this repo's README*

Balancing the game is done by changing the parameters exposed in the inspector. Almost every numbers in-game can be changed this way, and almost no text in-game are changeable this way. Here is how to do so:
1. **Clone this project**, preferably using GitHub Desktop
2. **Open the cloned project** in Unity. **BEWARE: Only open this project with Unity version 2018.1.1**. If this project is opened with other versions of Unity and get committed, the project **WILL HAVE TO BE RESTARTED** from the beginning!
3. **Open *Game* Scene**, which should be open by default. If it's not open, or a different scene is currently open, open it through the Projects panel > Scenes > Game
4. **Change values in the inspector**. Only change needed values. Do **NOT** change recklessly! Any changes you commit will be overwritten to everyone's copies, so make sure none of you changed the same parameters. Furthermore, do **NOT** change any parameters not mentioned here and any parameters that are **explicitly** forbidden to be changed!
5. **Commit said changes back** to GitHub. Once committed, unless you remember the original values, **there's no turning back** so make sure that the values are properly changed.
 
Below is a comprehensive guide to each parameters along with how to find and change them:

## Base Mechanics
### Starting Money

Found In | Type | Default
--- | --- | ---
Managers > GameManager | Float | 50

Changes the starting amount of money players will get on a new game. Does not affect old games.

### Customer Spawn Time

Found In | Type | Default
--- | --- | ---
Managers > GameManager | Float | 5

The amount of time, in seconds, to wait before spawning a customer, new or returning.

### Demand Keypoints

Found In | Type | Default
--- | --- | ---
Managers > MarketManager | List of Dynamics | Spreadsheet value at the time this document was updated

Sets the Customer's demand on a particular day.

This is an array value. Click the parameter name to expand until you see Element 0, Element 1, etc. Each element corresponds to a row in the Market Dynamics sheet where the Total is 100%. Inside each element is the Day - which is the day written in the spreadsheet - and the Dynamics of that day, which is another array value. **The size of the Dynamics array array must be 5.** The element number of the Dynamics array corresponds to the game:

Game | Element
--- | ---
A (Edwin's) | 0
B (Rius') | 1
C (Derrick's) | 2
D (Virya's) | 3
E (Asyraf's) | 4

To fill the Dynamics array, fill the **numerator** part of the fraction present in the Spreadsheet (so if in the spreadsheet it's written 4/9, you type 4)

### Sale/Buy Keypoints

Found In | Type | Default
--- | --- | ---
Managers > MarketManager | List of Dynamics | Spreadsheet value at the time this document was updated

Sets the Game's sell price (for selling to the customer) and buy price (to restock from suppliers) dynamics.

Filling this is the same as filling Demand Keypoints, just for buy/sell prices.

### Start Hour

Found In | Type | Default
--- | --- | ---
Managers > DaytimeManager | Integer | Equal to End Hour

Sets the time of day when starting a new game. This does not change the start business time of the day, which is currently hardcoded at 8 o'clock.

**This value must be the same or later than the End Hour!**

### End Hour

Found In | Type | Default
--- | --- | ---
Managers > DaytimeManager | Integer | 17

Sets the end business time, or the time when the business is closed and no longer accepting customers, marking the end of day. Must be less than 24 or else it will crash the game.

### Time Speed

Found In | Type | Default
--- | --- | ---
Managers > DaytimeManager | Float | 360

Sets the in-game time speed, in seconds/realtime seconds. A value of 1 here means that the game clock runs at the same speed as a real clock (don't do this).

## Customer Parameters
To change these parameters, open the customer prefab in Prefabs folder named Capsule. These parameters applies to all customers.

Note that these parameters are deprecated:
- **Base Visit Chance**: replaced by Demand Keypoints.

### Starting Happiness

Found In | Type | Default
--- | --- | ---
Prefab > Customer | Float | 60

Set the starting Happiness of the customer when entering the shop for the first time (new customer)

### Action Weight

Found In | Type | Default
--- | --- | ---
Prefab > Customer | Float Array | [4, 3, 1]

Set the probability weight of customer actions when they enter the shop. To get the probability in percent, add all elements of the array together, then divide each element by the total and multiply it by 100.

This is an array value. Click the parameter name to expand until you see Element 0, Element 1, etc. Each element corresponds to the following actions:

Element | Action
--- | ---
0 | **Shopping** the customer will buy a game according to current demands.
1 | **Wander** the customer will look around the shop, doing nothing for several seconds.
2 | **Complain** the customer will go tothe Customer Service to complain, increasing happiness.

### Look/Complain/Wander Time

Found In | Type | Default
--- | --- | ---
Prefab > Customer | Float | 4, 20 and 10 respectively 

Set the base duration of the customer looking/complaining/wandering. For Complain and Wander time, this is the total time spent complaining/wandering. For Look Time, this is the time for one of 2 stages of purchase: Looking at the Showcase and Purchasing the Game on the Cashier. Look and Complain time does not include walking time, while Wander Time does.

### Speed

Found In | Type | Default
--- | --- | ---
Prefab > NavMeshAgent | Float | 5

The base walking speed of the Customer. Affects how fast Customers move around, but does not affect their action speed, which is affected by the Look/Complain/Wander Time parameter above.

Note that when the customer is wandering, their speed is reduced to 75%

### Angular Speed

Found In | Type | Default
--- | --- | ---
Prefab > NavMeshAgent | Float | 180

The base turn speed of the Customer. Affects how fast Customers turn around. This parameter remains constant throughout the game.

Increasing Speed and Angular speed will make the game *seem* faster.

## Additional
### Customer Name File

Found In | Type | Default
--- | --- | ---
Managers > GameManager | Text | custnames

The file name or path to load a list of customer names from relative to the Resources folder. This file is loaded to give the customer a name every time it is spawned. The file needs to contain names separated with newlines (\n), and each name must be shorter than 20 characters.

File name does not include extensions.

### Pan/Rotate/Pinch Sensitivity

Found In | Type | Default
--- | --- | ---
Managers > GameManager | Float | 1

Adjust the sensitivity of each gesture. This setting is exposed to the player, so playing with this is not required.

### Game Start Title

Found In | Type | Default
--- | --- | ---
Main UI > EndDayPanel > EndDayManager | Text | "Pre-Business Setup"

The text shown on the title of the Day End overview panel when starting a new game. Can be any text shorter than 30 characters

### Day Start/End Title

Found In | Type | Default
--- | --- | ---
Main UI > EndDayPanel > EndDayManager | Text | "End of Day {0}"

The text shown on the title of the Day End overview panel on subsequent start/end of day after the day in which the Game Start Title is shown. {0} will be replaced by the script to the day number. Can be any text shorter than 30 characters, but the "{0}" (without quotes) is **required** so you effectively have 27 characters left.