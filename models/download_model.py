"""
OmniAgent Engine — GGUF Model Downloader

Downloads a compact GGUF Small Language Model (SLM) for local on-device inference.
Default: Microsoft Phi-4-mini-Instruct (3.8B, Q4_K_M ~2.4 GB)
"""

import argparse
import sys
import time
import urllib.request
from pathlib import Path

MODEL_DIR = Path(__file__).resolve().parent

# Available model presets
AVAILABLE_MODELS = {
    "phi-4-mini": {
        "name": "Microsoft Phi-4-mini-Instruct (3.8B, Q4_K_M)",
        "url": "https://huggingface.co/bartowski/microsoft_Phi-4-mini-instruct-GGUF/resolve/main/microsoft_Phi-4-mini-instruct-Q4_K_M.gguf",
        "filename": "phi-4-mini.gguf",
        "size_desc": "~2.4 GB",
    },
    "qwen-3b": {
        "name": "Qwen2.5-3B-Instruct (3.0B, Q4_K_M)",
        "url": "https://huggingface.co/Qwen/Qwen2.5-3B-Instruct-GGUF/resolve/main/qwen2.5-3b-instruct-q4_k_m.gguf",
        "filename": "phi-4-mini.gguf",  # saved as phi-4-mini.gguf for engine compatibility
        "size_desc": "~2.0 GB",
    },
    "qwen-7b": {
        "name": "Qwen2.5-7B-Instruct (7.0B, Q4_K_M)",
        "url": "https://huggingface.co/bartowski/Qwen2.5-7B-Instruct-GGUF/resolve/main/Qwen2.5-7B-Instruct-Q4_K_M.gguf",
        "filename": "phi-4-mini.gguf",
        "size_desc": "~4.5 GB",
    },
    "llama-3.2-3b": {
        "name": "Llama-3.2-3B-Instruct (3.2B, Q4_K_M)",
        "url": "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf",
        "filename": "phi-4-mini.gguf",
        "size_desc": "~2.0 GB",
    },
}

DEFAULT_MODEL_KEY = "phi-4-mini"
MODEL_URL = AVAILABLE_MODELS[DEFAULT_MODEL_KEY]["url"]
MODEL_PATH = MODEL_DIR / AVAILABLE_MODELS[DEFAULT_MODEL_KEY]["filename"]


class DownloadProgress:
    def __init__(self):
        self.start_time = time.time()
        self.last_update = 0

    def __call__(self, count, block_size, total_size):
        now = time.time()
        # Update display at most 10 times per second to prevent terminal lag
        if now - self.last_update < 0.1 and count * block_size < total_size:
            return
        self.last_update = now

        downloaded = count * block_size
        elapsed = max(now - self.start_time, 0.001)
        speed_mb = (downloaded / (1024 * 1024)) / elapsed

        if total_size > 0:
            percent = min(int(downloaded * 100 / total_size), 100)
            downloaded_mb = downloaded / (1024 * 1024)
            total_mb = total_size / (1024 * 1024)
            eta_sec = int((total_size - downloaded) / max(downloaded / elapsed, 1))
            eta_str = f"{eta_sec // 60:02d}:{eta_sec % 60:02d}"
            sys.stdout.write(
                f"\r[Downloading] {percent:3d}% ({downloaded_mb:.1f}/{total_mb:.1f} MB) "
                f"— {speed_mb:.1f} MB/s — ETA {eta_str}  "
            )
        else:
            downloaded_mb = downloaded / (1024 * 1024)
            sys.stdout.write(f"\r[Downloading] {downloaded_mb:.1f} MB ({speed_mb:.1f} MB/s)  ")
        sys.stdout.flush()


def download_model(model_key: str = DEFAULT_MODEL_KEY, force: bool = False):
    preset = AVAILABLE_MODELS.get(model_key)
    if not preset:
        print(f"❌ Unknown model preset: '{model_key}'. Available options:")
        for key, info in AVAILABLE_MODELS.items():
            print(f"  • {key:14s} -> {info['name']} ({info['size_desc']})")
        sys.exit(1)

    url = preset["url"]
    target_path = MODEL_DIR / preset["filename"]

    MODEL_DIR.mkdir(parents=True, exist_ok=True)

    if target_path.exists() and target_path.stat().st_size > 500_000_000 and not force:
        print(f"✅ GGUF model already present at: {target_path} ({target_path.stat().st_size / (1024*1024):.1f} MB)")
        print("Use --force to re-download if needed.")
        return

    print("=" * 60)
    print(f"🧠 OmniAgent GGUF Model Downloader")
    print(f"Model:  {preset['name']}")
    print(f"Size:   {preset['size_desc']}")
    print(f"Target: {target_path}")
    print(f"URL:    {url}")
    print("=" * 60 + "\n")

    opener = urllib.request.build_opener()
    opener.addheaders = [("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)")]
    urllib.request.install_opener(opener)

    progress = DownloadProgress()
    try:
        urllib.request.urlretrieve(url, target_path, reporthook=progress)
        print(f"\n\n✨ Successfully downloaded {preset['name']} to {target_path}!")
    except KeyboardInterrupt:
        print("\n\n⏸️ Download cancelled by user.")
    except Exception as e:
        print(f"\n\n❌ Error downloading model: {e}")
        print("Tip: You can manually place any compatible GGUF model into ./models/phi-4-mini.gguf")


def main():
    parser = argparse.ArgumentParser(description="Download local GGUF models for OmniAgent Engine.")
    parser.add_argument(
        "--model",
        "-m",
        choices=list(AVAILABLE_MODELS.keys()),
        default=DEFAULT_MODEL_KEY,
        help=f"Model preset to download (default: {DEFAULT_MODEL_KEY})",
    )
    parser.add_argument(
        "--force",
        "-f",
        action="store_true",
        help="Force re-download even if model file already exists",
    )
    parser.add_argument(
        "--list",
        "-l",
        action="store_true",
        help="List available model presets",
    )

    args = parser.parse_args()

    if args.list:
        print("\nAvailable model presets for OmniAgent:")
        for key, info in AVAILABLE_MODELS.items():
            mark = " (default)" if key == DEFAULT_MODEL_KEY else ""
            print(f"  • {key:14s}: {info['name']} [{info['size_desc']}]{mark}")
        print()
        return

    download_model(model_key=args.model, force=args.force)


if __name__ == "__main__":
    main()
