#!/usr/bin/env python3
"""
Deploy mood-selector.js to VPS using sshpass
Fallback: Manual deployment if sshpass not available
"""

import subprocess
import os
import sys
from pathlib import Path

def run_command(cmd, describe=""):
    """Run command and return output"""
    if describe:
        print(f"\n{describe}...")
    print(f"  $ {' '.join(cmd)}")
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.stdout:
        print(result.stdout)
    if result.stderr:
        print(result.stderr, file=sys.stderr)
    return result.returncode, result.stdout, result.stderr

def main():
    # Configuration
    vps_host = "188.245.68.164"
    vps_port = "2222"
    vps_user = "mimm"
    password = "MIMMpassword"
    web_root = "/var/www/mimm-frontend"
    
    local_file = "src/MIMM.Frontend/wwwroot/js/mood-selector.js"
    
    if not Path(local_file).exists():
        print(f"❌ {local_file} not found!")
        return 1
    
    # Check if sshpass is available
    try:
        run_command(["sshpass", "-V"], "Checking for sshpass")
        has_sshpass = True
    except:
        has_sshpass = False
    
    if has_sshpass:
        print("✅ Using sshpass for password authentication")
        
        # Upload with sshpass
        cmd = [
            "sshpass", "-p", password,
            "scp", "-P", str(vps_port),
            local_file, f"{vps_user}@{vps_host}:{web_root}/js/"
        ]
        code, out, err = run_command(cmd, "📤 Uploading mood-selector.js")
        
        if code != 0:
            print("❌ Upload failed!")
            return 1
        
        # Verify
        cmd = [
            "sshpass", "-p", password,
            "ssh", "-p", str(vps_port), f"{vps_user}@{vps_host}",
            f"ls -lh {web_root}/js/mood-selector.js && echo '✅ File deployed successfully'"
        ]
        code, out, err = run_command(cmd, "🔍 Verifying deployment")
        return code
    else:
        print("⚠️ sshpass not available - please install it or use manual method:")
        print("   Windows: choco install sshpass")
        print("   macOS: brew install sshpass")
        print("   Linux: apt install sshpass")
        print("\nManual method:")
        print(f"  scp -P {vps_port} {local_file} {vps_user}@{vps_host}:{web_root}/js/")
        return 1

if __name__ == "__main__":
    sys.exit(main())
