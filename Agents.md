# Agent Development Guidelines

## Asynchronous Operations

When implementing behavior that occurs over time, such as animations or actions with a duration, always prefer using the `Update` or `FixedUpdate` methods with state variables. Avoid using Coroutines.

**Reasoning:**

- **Centralized Logic:** `Update`-based logic keeps all of a component's behavior in one place, making it easier to read, debug, and reason about the component's state machine.
- **Predictable Execution:** The `Update` loop has a clear, consistent execution order within the frame, which helps prevent race conditions and other subtle timing bugs.
- **State Management:** It encourages explicit state management (e.g., using boolean flags or enums), which makes the component's current behavior transparent and less error-prone. Coroutines can have hidden state, making it difficult to know what the component is doing at any given time.
- **Easier Interruption:** Interrupting or overriding behavior is more straightforward. With an `Update`-based approach, you can simply change a state variable. Interrupting a coroutine can be more complex and may not properly clean up its state.

---

## UI Component Implementation Notes: `UITextArea` and `UIPanel`

When working with UI components like `UITextArea` and `UIPanel`, it is critical to:

1.  **Understand the Base Class's Intent (`UIPanel`)**:
    *   Do NOT assume a UI script is merely for displaying raw text. Always inspect its base classes (`UIPanel` in this case) and existing methods (e.g., `AddButton`).
    *   `UIPanel` is designed to be an interactive container for UI elements, providing methods like `AddButton` and managing common panel behaviors (like closing itself). Overwriting its core functionality or ignoring its exposed members is a critical error.
    *   Leverage the `scrollContent` Transform provided by `UIPanel` as the parent for dynamically instantiated UI elements.

2.  **Correctly Utilize Prefabs (e.g., `UITextArea`)**:
    *   If a UI element (like `UITextArea`) is intended to be dynamically instantiated, it should be set up as a **prefab**.
    *   **Avoid Runtime `GameManager` Dependency for Prefab Loading in Different Scenes**: If a UI panel exists in a scene *without* the `GameManager` (e.g., a "MainMenu" scene), do NOT attempt to load prefabs via `GameManager.Instance.prefabManager.GetPrefab()`. `GameManager.Instance` will be `null`, leading to `NullReferenceException`.
    *   **Direct Inspector Assignment**: For prefabs required by UI components in scenes without the `GameManager`, add a `public GameObject yourPrefabName;` field to the UI script (e.g., `UIMetaChallengesPanel.cs`). This allows the prefab to be directly assigned in the Unity Editor's Inspector from the Project window, making the dependency explicit and preventing runtime errors.

3.  **Clear Dynamic Content**:
    *   When dynamically adding UI elements (e.g., buttons or text areas) to a `scrollContent` area in an `OnEnable()` method, always ensure you **clear existing child GameObjects** from `scrollContent` first. This prevents duplicate elements from appearing each time the panel is enabled. Use a loop like `foreach (Transform child in scrollContent) { Destroy(child.gameObject); }`.

4.  **Displaying Data**:
    *   Use `StringBuilder` for efficient string concatenation when formatting multiple lines of text for a `UITextArea`.
    *   Be mindful of rich text tags (e.g., `<color>`, `<size>`, `<i>`) for improved readability and visual presentation.

**Lesson Learned (from this interaction):** Read existing code, especially base classes and their public methods, *before* attempting to implement new features. Validate assumptions about how GameObjects and prefabs are accessed across different Unity scenes. Always double-check the user's specific request and avoid introducing unrelated logic.