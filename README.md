# Holy Item Editor
### Scriptable object creation and modification tool base for Unity


<img width="897" height="601" alt="Unity_HqQi2xK8UU" src="https://github.com/user-attachments/assets/d989d651-d7b8-45a0-954b-df84c735bb81" />

When games use many ScriptableObject items, it can get really tedious to find and edit items through project window.

For this purpose, I creted my own tool, but it was hard to implement it into other projects, so i made a clean base version which is easily extendable for your specific needs.


### How To Implement
1. Import the package to your project
2. Import the sample packages

   <img width="744" height="414" alt="image" src="https://github.com/user-attachments/assets/4abfc003-128e-4375-bc3a-0df56d7366a1" />
3. Open the window via Tool > Holylib > HolyItemEditor

   <img width="469" height="117" alt="image" src="https://github.com/user-attachments/assets/237dd6e0-4329-406a-ad42-c7ce9a8e90eb" />

You are ready to go!

### How to Extend
- If you want to put your own item data you can either modify the **StaticItemData** script or change the references of it in the public scripts.
- You can modify every script in BasicItemEditor folder inside Samples folder, they are made for this.

#### Search Utilities
You can use the search bar to search more than the name of the items, you can chain sql queries.
But of course every item needs a unique query, you can implement your own query functions in **SearchFeatures** script


## If you are curious how my personal version looks like you can check this repo:
https://github.com/omy181/Editor-Item-Creator-Tool
