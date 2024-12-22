using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TeamXClient
{
    public class TeamXPanelComponent
    {
        public TeamXPanelComponentType ComponentType;
        public TeamXPanelComponentName ComponentName;
        public LEV_CustomButton Button;
        public Image Image;
        public Image textInputBackground;
        public RectTransform Rect;
        public TextMeshProUGUI textMesh;
        public ScrollRect ScrollRect;
        public TMP_InputField textInputField;
        public RectTransform explorerPanel;
        public ContentSizeFitter contentSizeFitter;
        public GridLayoutGroup gridLayoutGroup;

        public TeamXPanelComponent(TeamXPanelComponentType componentType, TeamXPanelComponentName componentName, RectTransform rect)
        {
            this.ComponentType = componentType;
            this.ComponentName = componentName;
            this.Rect = rect;
            this.Rect.gameObject.name = componentName.ToString();

            switch (ComponentType)
            {
                case TeamXPanelComponentType.Button:
                    Button = rect.GetComponent<LEV_CustomButton>();
                    if (Button != null)
                    {
                        InterfaceManager.UnbindButton(Button);
                        InterfaceManager.StandardRecolorButton(Button);

                        Transform label = Button.transform.Find("Label");
                        if(label != null)
                        {
                            TextMeshProUGUI tmpLabel = label.GetComponent<TextMeshProUGUI>();
                            if(tmpLabel != null)
                            {
                                textMesh = tmpLabel;

                                Localize loc2 = tmpLabel.GetComponent<Localize>();
                                if (loc2 != null)
                                {
                                    GameObject.Destroy(loc2);
                                }
                            }
                        }
                    }

                    break;
                case TeamXPanelComponentType.Image:
                    Image = rect.GetComponent<Image>();
                    break;
                case TeamXPanelComponentType.Text:
                    textMesh = rect.GetComponent<TextMeshProUGUI>();
                    textMesh.text = "";
                    break;
                case TeamXPanelComponentType.ScrollView:
                    ScrollRect = rect.GetComponent<ScrollRect>();

                    // Configuring the ScrollRect as per the standalone code
                    explorerPanel = ScrollRect.content;
                    contentSizeFitter = explorerPanel.gameObject.GetComponent<ContentSizeFitter>();
                    if (contentSizeFitter == null)
                    {
                        contentSizeFitter = explorerPanel.gameObject.AddComponent<ContentSizeFitter>();
                    }
                    contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    gridLayoutGroup = explorerPanel.gameObject.GetComponent<GridLayoutGroup>();
                    if (gridLayoutGroup == null)
                    {
                        gridLayoutGroup = explorerPanel.gameObject.AddComponent<GridLayoutGroup>();
                    }
                    gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    SetGridLayoutColumns(6);
                    break;
                case TeamXPanelComponentType.TextInput:
                    textInputField = rect.GetComponent<TMP_InputField>();

                    textInputBackground = rect.GetComponent<Image>();
                    if (textInputBackground != null)
                    {
                        textInputBackground.color = Color.white;
                    }

                    Localize loc = textInputField.placeholder.GetComponent<Localize>();
                    if (loc != null)
                    {
                        GameObject.Destroy(loc);
                    }

                    SetPlaceHolderText("");
                    break;
            }
        }

        public void SetGridLayoutColumns(int count, float heightRatio = 1f)
        {
            if (ComponentType != TeamXPanelComponentType.ScrollView)
            {
                return;
            }

            gridLayoutGroup.constraintCount = count;

            Rect viewportRect = ScrollRect.viewport.rect;
            int paddingValue = Mathf.RoundToInt(viewportRect.width / 100f);
            float cellWidth = (viewportRect.width - paddingValue * 2) / ((float)count);
            float cellSpacing = cellWidth * 0.05f;

            gridLayoutGroup.cellSize = new Vector2(cellWidth - cellSpacing, (cellWidth - cellSpacing) * heightRatio);
            gridLayoutGroup.spacing = new Vector2(cellSpacing, cellSpacing);
            gridLayoutGroup.padding = new RectOffset(paddingValue, paddingValue, paddingValue, paddingValue);
        }

        public void Reset()
        {
            switch (ComponentType)
            {
                case TeamXPanelComponentType.Button:
                    Button.ResetAllBools();
                    break;
            }
        }

        public void Enable()
        {
            Rect.gameObject.SetActive(true);
        }

        public void Disable()
        {
            Rect.gameObject.SetActive(false);
        }

        public void SetInteractable(bool state)
        {
            if (ComponentType == TeamXPanelComponentType.TextInput)
            {
                textInputField.interactable = state;
            }
        }

        public void BindButton(UnityAction action)
        {
            if (ComponentType == TeamXPanelComponentType.Button)
            {
                InterfaceManager.RebindButton(Button, action);
            }
        }

        public void SetRectAnchors(float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY)
        {
            Rect.anchorMin = new Vector2(anchorMinX, anchorMinY);
            Rect.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        }

        public void SetButtonImageRectAnchors(float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY)
        {
            if (ComponentType == TeamXPanelComponentType.Button)
            {
                RectTransform imageChild = Rect.GetChild(0).GetComponent<RectTransform>();
                imageChild.anchorMin = new Vector2(anchorMinX, anchorMinY);
                imageChild.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
            }
        }

        public void ColorImage(Color color)
        {
            if (ComponentType == TeamXPanelComponentType.Image)
            {
                Image.color = color;
            }
        }

        public void SetText(string text)
        {
            if (ComponentType == TeamXPanelComponentType.Text)
            {
                textMesh.text = text;
            }
            else if (ComponentType == TeamXPanelComponentType.TextInput)
            {
                textInputField.text = text;
            }
        }

        public void SetPlaceHolderText(string text)
        {
            if (ComponentType == TeamXPanelComponentType.TextInput)
            {
                textInputField.placeholder.GetComponent<TMP_Text>().text = text;
            }
        }

        public string GetText()
        {
            if (ComponentType == TeamXPanelComponentType.TextInput)
            {
                return textInputField.text;
            }

            return "";
        }

        public void SetButtonImage(Sprite sprite)
        {
            Rect.GetChild(0).GetComponent<Image>().sprite = sprite;
        }

        public void HideButtonImage()
        {
            Transform r = Rect.GetChild(0);
            if(r != null)
            {
                r.gameObject.SetActive(false);
            }
        }

        public void SetButtonText(string text)
        {
            if (ComponentType == TeamXPanelComponentType.Button)
            {
                if(textMesh != null)
                {
                    textMesh.text = text;
                }
            }
        }

        public void HideButtonText()
        {
            if (ComponentType == TeamXPanelComponentType.Button)
            {
                TextMeshProUGUI textGUI = Button.GetComponentInChildren<TextMeshProUGUI>();
                if (textGUI != null)
                {
                    textGUI.gameObject.SetActive(false);
                }
            }
        }
    }
}
