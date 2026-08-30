# Holy Item Editor
### Extendable ScriptableObject creation and modification tool for Unity

<img width="985" height="575" alt="Unity_fPcXMC5JAA" src="https://github.com/user-attachments/assets/5344ad9a-3002-4802-8826-450fe1353747" />

When games use many ScriptableObject items, it can get really tedious to find and edit items through project window.

For this purpose, I creted my own tool, but it was hard to implement it into other projects, so i made a clean version which is easily extendable for your specific needs.

## Features
- Custom ScriptableObject search based on content
- Detailed chainable search
- Multiple Types of ScriptableObjects in single list
- Customizable ScriptableObject viewer

### How To Implement
1. Import the package to your project
2. Import the sample packages

   <img width="735" height="408" alt="image" src="https://github.com/user-attachments/assets/ba0b8582-8278-42a7-85fa-c20a8f519481" />
3. Open the window via Tool > Holylib > HolyItemEditor

   <img width="469" height="117" alt="image" src="https://github.com/user-attachments/assets/237dd6e0-4329-406a-ad42-c7ce9a8e90eb" />

You are ready to go!

### How to add your own ScriptableObject types
1. Find **ItemManagementReferences.cs** in the Samples folder
2. Put your own ScriptableObjects here
   ```C#
   public static readonly ItemListElementAndPath[] SupportedItemListTypes = { 
      new( typeof(StaticItemData), "Assets/Resources/Items",Color.azure), 
      new( typeof(RecipeData), "Assets/Resources/Recipes",Color.yellow) };

   // The path you enter is for newly created ScriptableObjects
   // And Color will be visible next to the items in the item list
   ```
3. Your ScriptableObjects need to implement the **ItemListElement** interface
   ```C#
    public interface ItemListElement
    {
        public ItemListData GetValues();
        public void InitializeValues(string id, string name);
        public ElementPreviewData PreviewElement();
        public SearchQuery[] GetCustomSearchLogic();
    }
   ```
   There are example Item scripts in the Samples folder



#### Custom Search
You can use the search bar to search more than the name of the items, you can chain sql queries.
But of course every item needs a unique query, you can implement your own query functions in you ScriptableObjects through **GetCustomSearchLogic()** function

#### Custom Object Preview
You can modify the **PreviewElement()** function in the ItemListElement interface in any way to place whatever you want to right side of the editor.

## If you are curious how my personal version looks like you can check this repo:
https://github.com/omy181/Editor-Item-Creator-Tool
