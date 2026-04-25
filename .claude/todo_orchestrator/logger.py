import logging
import sys

try:
    from rich.console import Console
    from rich.logging import RichHandler

    _console = Console(stderr=True)
    logging.basicConfig(
        level=logging.INFO,
        format="%(message)s",
        datefmt="[%X]",
        handlers=[RichHandler(console=_console, rich_tracebacks=True, show_path=False)],
    )
    _has_rich = True
except ImportError:
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
        stream=sys.stderr,
    )
    _has_rich = False


def get_logger(name: str) -> logging.Logger:
    return logging.getLogger(name)


def print_header(text: str) -> None:
    if _has_rich:
        from rich.console import Console
        Console().rule(f"[bold blue]{text}[/bold blue]")
    else:
        print(f"\n{'='*60}\n  {text}\n{'='*60}")


def print_success(text: str) -> None:
    if _has_rich:
        from rich.console import Console
        Console().print(f"[bold green]✓ {text}[/bold green]")
    else:
        print(f"✓ {text}")


def print_warning(text: str) -> None:
    if _has_rich:
        from rich.console import Console
        Console().print(f"[bold yellow]⚠ {text}[/bold yellow]")
    else:
        print(f"⚠ {text}")
