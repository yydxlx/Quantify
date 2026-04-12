using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class StockUIEditor
{
    [MenuItem("Tools/创建股票查询UI(完整版)")]
    public static void CreateCompleteStockUI()
    {
        // 创建Canvas
        GameObject CanvasObj = new GameObject("StockCanvas");
        Canvas Canvas = CanvasObj.AddComponent<Canvas>();
        Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasObj.AddComponent<CanvasScaler>();
        CanvasObj.AddComponent<GraphicRaycaster>();

        // 创建主面板
        GameObject MainPanel = CreatePanel(Canvas.transform, "MainPanel", new Vector2(900, 700));

        // 创建标题（代码中未使用）
        CreateText(MainPanel.transform, "titleText", "股票数据查询分析系统",
                  new Vector2(0, 300), new Vector2(400, 60), 28, TextAnchor.MiddleCenter);

        // 创建Toggle组（代码中未使用）
        GameObject toggleGroup = CreatePanel(MainPanel.transform, "toggleGroup", new Vector2(400, 50));
        toggleGroup.transform.localPosition = new Vector3(0, 220, 0);

        // 数据获取Toggle（代码中使用）
        GameObject DataFetchToggle = CreateToggle(toggleGroup.transform, "DataFetchToggle",
                  new Vector2(-100, 0), new Vector2(160, 30), "数据获取");
        DataFetchToggle.GetComponent<Toggle>().isOn = true;

        // 数据分析Toggle（代码中使用）
        GameObject DataAnalyzeToggle = CreateToggle(toggleGroup.transform, "DataAnalyzeToggle",
                  new Vector2(100, 0), new Vector2(160, 30), "数据分析");

        // 输入区域（代码中未使用）
        GameObject inputGroup = CreatePanel(MainPanel.transform, "inputGroup", new Vector2(800, 100));
        inputGroup.transform.localPosition = new Vector3(0, 150, 0);

        // 日期输入（代码中使用）
        CreateText(inputGroup.transform, "dateLabel", "交易日期:",
                  new Vector2(-350, 30), new Vector2(120, 30), 16, TextAnchor.MiddleLeft);
        GameObject DateInputField = CreateInputField(inputGroup.transform, "DateInput",
                  new Vector2(-200, 30), new Vector2(150, 30), "yyyyMMdd");

        // 股票代码输入（代码中使用）
        CreateText(inputGroup.transform, "stockLabel", "股票代码:",
                  new Vector2(-350, -20), new Vector2(120, 30), 16, TextAnchor.MiddleLeft);
        GameObject SingleStockInput = CreateInputField(inputGroup.transform, "StockInput",
                  new Vector2(-200, -20), new Vector2(150, 30), "000001.SZ");

        // 天数输入（代码中使用）
        CreateText(inputGroup.transform, "daysLabel", "查询天数:",
                  new Vector2(0, -20), new Vector2(120, 30), 16, TextAnchor.MiddleLeft);
        GameObject DaysInputField = CreateInputField(inputGroup.transform, "DaysInput",
                  new Vector2(150, -20), new Vector2(100, 30), "30");

        // 数据获取面板（代码中使用）
        GameObject DataFetchPanel = CreatePanel(MainPanel.transform, "DataFetchPanel", new Vector2(800, 120));
        DataFetchPanel.transform.localPosition = new Vector3(0, 80, 0);

        // 按钮组（代码中未使用）
        GameObject buttonGroup = CreatePanel(DataFetchPanel.transform, "buttonGroup", new Vector2(600, 60));
        buttonGroup.transform.localPosition = new Vector3(0, 0, 0);

        // 获取全部30日数据按钮（代码中使用）
        GameObject FetchAll30DaysButton = CreateButton(buttonGroup.transform, "FetchAll30DaysButton",
                  new Vector2(-200, 0), new Vector2(180, 40), "获取全部30日数据");

        // 获取全部60日数据按钮（代码中使用）
        GameObject FetchAll60DaysButton = CreateButton(buttonGroup.transform, "FetchAll60DaysButton",
                  new Vector2(0, 0), new Vector2(180, 40), "获取全部60日数据");

        // 获取单个股票数据按钮（代码中使用）
        GameObject FetchSingleStockButton = CreateButton(buttonGroup.transform, "FetchSingleStockButton",
                  new Vector2(200, 0), new Vector2(180, 40), "获取单个股票数据");

        // 数据分析面板（代码中使用）
        GameObject DataAnalyzePanel = CreatePanel(MainPanel.transform, "DataAnalyzePanel", new Vector2(800, 120));
        DataAnalyzePanel.transform.localPosition = new Vector3(0, 80, 0);
        DataAnalyzePanel.SetActive(false);

        // 分析跌幅超5%按钮（代码中使用）
        GameObject AnalyzeDrop5PercentButton = CreateButton(DataAnalyzePanel.transform, "AnalyzeDrop5PercentButton",
                  new Vector2(0, 0), new Vector2(200, 40), "分析跌幅超5%股票");

        // 状态显示（代码中使用）
        GameObject StatusText = CreateText(MainPanel.transform, "StatusText", "就绪",
                  new Vector2(0, 20), new Vector2(800, 30), 14, TextAnchor.MiddleLeft);

        // 结果显示区域（代码中未使用）
        GameObject resultPanel = CreatePanel(MainPanel.transform, "ResultPanel", new Vector2(850, 350));
        resultPanel.transform.localPosition = new Vector3(0, -150, 0);

        // 结果内容（代码中使用）
        GameObject ResultContent = CreateScrollView(resultPanel.transform, "ResultContent",
                  new Vector2(0, 0), new Vector2(830, 330));

        // 进度条组（代码中使用）
        GameObject ProgressPanel = CreatePanel(MainPanel.transform, "ProgressGroup", new Vector2(400, 40));
        ProgressPanel.transform.localPosition = new Vector3(0, -320, 0);
        ProgressPanel.SetActive(false);

        // 进度条滑块（代码中使用）
        GameObject ProgressSlider = CreateSlider(ProgressPanel.transform, "ProgressSlider",
                  new Vector2(0, 0), new Vector2(300, 20));

        // 进度文本（代码中使用）
        GameObject ProgressText = CreateText(ProgressPanel.transform, "ProgressText", "0%",
                  new Vector2(160, 0), new Vector2(60, 20), 14, TextAnchor.MiddleCenter);

    }

    // 创建面板
    private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        panel.transform.localPosition = Vector3.zero;
        panel.transform.localScale = Vector3.one;

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.sizeDelta = size;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);

        return panel;
    }

    // 创建文本
    private static GameObject CreateText(Transform parent, string name, string content,
                                       Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.localPosition = position;
        rt.sizeDelta = size;

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.black;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        return textObj;
    }

    // 创建输入框
    private static GameObject CreateInputField(Transform parent, string name,
                                             Vector2 position, Vector2 size, string placeholderText)
    {
        GameObject inputObj = new GameObject(name);
        inputObj.transform.SetParent(parent);

        RectTransform rt = inputObj.AddComponent<RectTransform>();
        rt.localPosition = position;
        rt.sizeDelta = size;

        Image image = inputObj.AddComponent<Image>();
        image.color = Color.white;

        InputField inputField = inputObj.AddComponent<InputField>();

        // 占位符文本（代码中未使用）
        GameObject placeholder = CreateText(inputObj.transform, "placeholder", placeholderText,
                                           Vector2.zero, Vector2.zero, 14, TextAnchor.MiddleLeft);
        placeholder.GetComponent<Text>().color = new Color(0.5f, 0.5f, 0.5f);

        RectTransform placeholderRT = placeholder.GetComponent<RectTransform>();
        placeholderRT.anchorMin = Vector2.zero;
        placeholderRT.anchorMax = Vector2.one;
        placeholderRT.offsetMin = new Vector2(10, 0);
        placeholderRT.offsetMax = new Vector2(-10, 0);

        // 输入文本（代码中未使用）
        GameObject textObj = CreateText(inputObj.transform, "text", "",
                                       Vector2.zero, Vector2.zero, 14, TextAnchor.MiddleLeft);
        textObj.GetComponent<Text>().color = Color.black;

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10, 0);
        textRT.offsetMax = new Vector2(-10, 0);

        inputField.placeholder = placeholder.GetComponent<Text>();
        inputField.textComponent = textObj.GetComponent<Text>();

        return inputObj;
    }

    // 创建按钮
    private static GameObject CreateButton(Transform parent, string name,
                                         Vector2 position, Vector2 size, string buttonText)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);

        RectTransform rt = buttonObj.AddComponent<RectTransform>();
        rt.localPosition = position;
        rt.sizeDelta = size;

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        // 按钮文本（代码中未使用）
        GameObject textObj = CreateText(buttonObj.transform, "text", buttonText,
                                       Vector2.zero, Vector2.zero, 16, TextAnchor.MiddleCenter);
        textObj.GetComponent<Text>().color = Color.white;

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;

        return buttonObj;
    }

    // 创建Toggle
    private static GameObject CreateToggle(Transform parent, string name,
                                         Vector2 position, Vector2 size, string toggleText)
    {
        GameObject toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent);

        RectTransform rt = toggleObj.AddComponent<RectTransform>();
        rt.localPosition = position;
        rt.sizeDelta = size;

        Toggle toggle = toggleObj.AddComponent<Toggle>();

        // 背景（代码中未使用）
        GameObject background = new GameObject("background");
        background.transform.SetParent(toggleObj.transform);
        RectTransform bgRT = background.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.8f, 0.8f, 0.8f);

        // 勾选框（代码中未使用）
        GameObject checkmark = new GameObject("checkmark");
        checkmark.transform.SetParent(background.transform);
        RectTransform checkRT = checkmark.AddComponent<RectTransform>();
        checkRT.anchorMin = new Vector2(0, 0);
        checkRT.anchorMax = new Vector2(1, 1);
        checkRT.sizeDelta = Vector2.zero;

        Image checkImage = checkmark.AddComponent<Image>();
        checkImage.color = new Color(0.2f, 0.8f, 0.2f);

        toggle.graphic = checkImage;
        toggle.targetGraphic = bgImage;

        // 标签文本（代码中未使用）
        GameObject labelObj = CreateText(toggleObj.transform, "label", toggleText,
                                        new Vector2(90, 0), new Vector2(120, 30), 14, TextAnchor.MiddleLeft);

        return toggleObj;
    }

    // 创建滚动视图
    private static GameObject CreateScrollView(Transform parent, string name,
                                             Vector2 position, Vector2 size)
    {
        GameObject scrollObj = new GameObject(name);
        scrollObj.transform.SetParent(parent);

        RectTransform rt = scrollObj.AddComponent<RectTransform>();
        rt.localPosition = position;
        rt.sizeDelta = size;

        Image image = scrollObj.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.5f);

        ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        // 视口（代码中未使用）
        GameObject viewport = new GameObject("viewport");
        viewport.transform.SetParent(scrollObj.transform);
        RectTransform viewportRT = viewport.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.sizeDelta = new Vector2(-20, 0);
        viewportRT.pivot = new Vector2(0.5f, 1f);

        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.2f);
        viewport.AddComponent<Mask>();

        // 内容区域（代码中使用）
        GameObject content = new GameObject("content");
        content.transform.SetParent(viewport.transform);
        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.5f, 1f);
        contentRT.anchorMax = new Vector2(0.5f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = new Vector2(size.x - 40f, 1000f);

        // 结果文本（代码中使用）
        GameObject ResultText = CreateText(content.transform, "Text", "查询结果将显示在这里",
                                       new Vector2(0, -500), new Vector2(size.x - 60f, 2000f), 12, TextAnchor.UpperLeft);
        ResultText.GetComponent<Text>().alignment = TextAnchor.UpperLeft;

        scrollRect.content = contentRT;
        scrollRect.viewport = viewportRT;

        return scrollObj;
    }

    // 创建滑块
    private static GameObject CreateSlider(Transform parent, string name,
                                         Vector2 position, Vector2 size)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent);

        RectTransform rt = sliderObj.AddComponent<RectTransform>();
        rt.localPosition = position;
        rt.sizeDelta = size;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0;

        // 背景（代码中未使用）
        GameObject background = new GameObject("background");
        background.transform.SetParent(sliderObj.transform);
        RectTransform bgRT = background.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.sizeDelta = Vector2.zero;

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.8f, 0.8f, 0.8f);

        // 填充区域（代码中未使用）
        GameObject fillArea = new GameObject("fillArea");
        fillArea.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1, 0.75f);
        fillAreaRT.sizeDelta = Vector2.zero;

        // 填充（代码中未使用）
        GameObject fill = new GameObject("fill");
        fill.transform.SetParent(fillArea.transform);
        RectTransform fillRT = fill.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.sizeDelta = Vector2.zero;

        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 0.2f);

        slider.fillRect = fillRT;

        // 手柄区域（代码中未使用）
        GameObject handleArea = new GameObject("handleArea");
        handleArea.transform.SetParent(sliderObj.transform);
        RectTransform handleAreaRT = handleArea.AddComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero;
        handleAreaRT.anchorMax = Vector2.one;
        handleAreaRT.sizeDelta = Vector2.zero;

        // 手柄（代码中未使用）
        GameObject handle = new GameObject("handle");
        handle.transform.SetParent(handleArea.transform);
        RectTransform handleRT = handle.AddComponent<RectTransform>();
        handleRT.anchorMin = new Vector2(0, 0);
        handleRT.anchorMax = new Vector2(0, 1f);
        handleRT.sizeDelta = new Vector2(20, 0);

        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;

        slider.handleRect = handleRT;

        return sliderObj;
    }
}