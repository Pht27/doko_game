from todo_orchestrator.logger import get_logger, print_header, print_warning

log = get_logger(__name__)


def ask_approval(prompt: str, context: str = "") -> bool:
    """Pause and ask the user for approval. Returns True if approved."""
    print_header(prompt)
    if context:
        print(context)
    while True:
        answer = input("\n  Approve? [y/n] ").strip().lower()
        if answer in ("y", "yes"):
            return True
        if answer in ("n", "no"):
            return False
        print_warning("Please enter 'y' or 'n'.")
