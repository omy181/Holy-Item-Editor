#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Holylib.ItemEditor
{
    public class CreateItemPopup : EditorWindow
    {
        private TextField _nameField;
        private Label _warningLabel;
        private System.Action<string> _onCreate;
        private System.Func<string, bool> _isValidName;

        public static void Show(System.Action<string> onCreate, System.Func<string, bool> isValidName)
        {
            var wnd = CreateInstance<CreateItemPopup>();
            wnd.titleContent = new GUIContent("Create New Item");
            wnd._onCreate = onCreate;
            wnd._isValidName = isValidName;

            Vector2 windowSize = new Vector2(260, 130);
            Vector2 mouseScreenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            Vector2 topLeft = new Vector2(
                mouseScreenPos.x - (windowSize.x / 2f), 
                mouseScreenPos.y - (windowSize.y / 2f)
            );

            wnd.ShowPopup();
            wnd.position = new Rect(topLeft, windowSize);
            wnd.Focus();
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingLeft = 10;
            root.style.paddingTop = 10;
            root.style.paddingRight = 10;
            root.style.paddingBottom = 10;
            root.style.alignItems = Align.Stretch;
            root.style.justifyContent = Justify.Center;

            _nameField = new TextField("Item Name");
            root.Add(_nameField);

            _warningLabel = new Label("");
            _warningLabel.style.color = Color.red;
            _warningLabel.style.display = DisplayStyle.None;
            root.Add(_warningLabel);

            var button = new Button(OnCreatePressed) { text = "Create" };
            root.Add(button);
        }

        private void OnCreatePressed()
        {
            var name = _nameField.value.Trim();
            if (string.IsNullOrEmpty(name))
            {
                ShowWarning("Name cannot be empty.");
                return;
            }

            if (!_isValidName(name))
            {
                ShowWarning("This name already exists.");
                return;
            }

            _onCreate?.Invoke(name);
            Close();
        }

        private void ShowWarning(string message)
        {
            _warningLabel.text = message;
            _warningLabel.style.display = DisplayStyle.Flex;
        }

        private void OnLostFocus() => Close();
    }
    public class DeleteItemPopup : EditorWindow
    {
        private Label _messageLabel;
        private System.Action _onDelete;

        private string _pendingItemName;
        private Rect _pendingPosition;

        public static void Show(string itemName, Vector2 screenPos, System.Action onDelete)
        {
            var wnd = CreateInstance<DeleteItemPopup>();
            wnd.titleContent = new GUIContent("Delete Item");
            wnd._onDelete = onDelete;
            wnd._pendingItemName = itemName;
            wnd._pendingPosition = new Rect(screenPos, new Vector2(250, 100));

            wnd.ShowPopup();
            wnd.position = wnd._pendingPosition;
            wnd.Focus();
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingLeft = 10;
            root.style.paddingTop = 10;
            root.style.paddingRight = 10;
            root.style.paddingBottom = 10;

            _messageLabel = new Label();
            root.Add(_messageLabel);

            // Now it's safe to set the text — the label exists.
            _messageLabel.text = $"Are you sure you want to delete '{_pendingItemName}'?";

            var buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Row;
            buttonsContainer.style.justifyContent = Justify.SpaceEvenly;

            var deleteButton = new Button(() =>
            {
                _onDelete?.Invoke();
                Close();
            })
            { text = "Delete" };
            buttonsContainer.Add(deleteButton);

            var cancelButton = new Button(() => Close()) { text = "Cancel" };
            buttonsContainer.Add(cancelButton);

            root.Add(buttonsContainer);
        }

        private void OnLostFocus() => Close();
    }
    public class MessagePopup : EditorWindow
    {
        private Label _messageLabel;
        private System.Action _onDelete;

        public static void Show(string message)
        {
            var wnd = CreateInstance<MessagePopup>();
            wnd.titleContent = new GUIContent("Message");

            var style = new GUIStyle(EditorStyles.label);
            Vector2 textSize = style.CalcSize(new GUIContent(message));

            Vector2 windowSize = textSize + new Vector2(40, 40);

            wnd.ShowPopup();
            wnd.position = new Rect(
                GUIUtility.GUIToScreenPoint(Event.current.mousePosition),
                windowSize
            );

            wnd._messageLabel.text = message;
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingLeft = 10;
            root.style.paddingTop = 10;
            root.style.paddingRight = 10;
            root.style.paddingBottom = 10;

            _messageLabel = new Label();
            root.Add(_messageLabel);

            var buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Row;
            buttonsContainer.style.justifyContent = Justify.SpaceEvenly;

            root.Add(buttonsContainer);
        }

        private void OnLostFocus() => Close();
    }
    public class MenuPopup : EditorWindow
    {
        private Label _messageLabel;
        private VisualElement _buttonsContainer;
        private List<(string name, Action onClick)> _buttons;

        private const float ButtonSpacing = 2f;
        private const float ButtonHorizontalPadding = 50f; 
        private const float WindowPadding = 10f;
        private string _message;
        public static void Show(string title, List<(string name, Action onClick)> buttons)
        {
            var wnd = CreateInstance<MenuPopup>();
            wnd.titleContent = new GUIContent(title);
            wnd._buttons = buttons;

            var titleStyle = new GUIStyle(EditorStyles.label);
            Vector2 titleSize = titleStyle.CalcSize(new GUIContent(title));

            var buttonStyle = new GUIStyle(EditorStyles.miniButton);
            float maxButtonWidth = 0f;
            float totalButtonHeight = 16f;

            foreach (var button in buttons)
            {
                Vector2 size = buttonStyle.CalcSize(new GUIContent(button.name));
                maxButtonWidth = Mathf.Max(maxButtonWidth, size.x + ButtonHorizontalPadding);
                totalButtonHeight += size.y + ButtonSpacing;
            }

            float contentWidth = Mathf.Max(titleSize.x, maxButtonWidth);
            float contentHeight = titleSize.y + totalButtonHeight;

            Vector2 windowSize = new Vector2(contentWidth, contentHeight) + new Vector2(WindowPadding * 2, WindowPadding * 2);

            wnd._message = title;
            wnd.ShowPopup();
            wnd.position = new Rect(
                GUIUtility.GUIToScreenPoint(Event.current.mousePosition),
                windowSize
            );
            wnd.Focus();
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingLeft = 15;
            root.style.paddingTop = 15;
            root.style.paddingRight = 15;
            root.style.paddingBottom = 15;

            _messageLabel = new Label();
            root.Add(_messageLabel);

            _buttonsContainer = new VisualElement();
            _buttonsContainer.style.flexDirection = FlexDirection.Column;
            _buttonsContainer.style.justifyContent = Justify.FlexStart;
            _buttonsContainer.style.alignItems = Align.Stretch;
            _buttonsContainer.style.flexGrow = 1;

            var title = new Label(_message);
            title.style.alignSelf = Align.Center;
            title.style.paddingBottom = 4;
            root.Add(title);
            root.Add(_buttonsContainer);


            for (int i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                var nb = new Button();
                nb.text = button.name;
                nb.RegisterCallback<MouseUpEvent>((e) => { button.onClick(); Close(); });

                if (i < _buttons.Count - 1)
                    nb.style.marginBottom = ButtonSpacing;

                _buttonsContainer.Add(nb);
            }
        }

        private void OnLostFocus() => Close();
    }
}
#endif