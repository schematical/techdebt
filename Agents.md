# Agent Development Guidelines

## Asynchronous Operations

When implementing behavior that occurs over time, such as animations or actions with a duration, always prefer using the `Update` or `FixedUpdate` methods with state variables. Avoid using Coroutines.

**Reasoning:**

- **Centralized Logic:** `Update`-based logic keeps all of a component's behavior in one place, making it easier to read, debug, and reason about the component's state machine.
- **Predictable Execution:** The `Update` loop has a clear, consistent execution order within the frame, which helps prevent race conditions and other subtle timing bugs.
- **State Management:** It encourages explicit state management (e.g., using boolean flags or enums), which makes the component's current behavior transparent and less error-prone. Coroutines can have hidden state, making it difficult to know what the component is doing at any given time.
- **Easier Interruption:** Interrupting or overriding behavior is more straightforward. With an `Update`-based approach, you can simply change a state variable. Interrupting a coroutine can be more complex and may not properly clean up its state.
