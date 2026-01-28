#!/usr/bin/env python3
"""
Deploy mood-selector.js using paramiko (SSH library for Python)
"""

import paramiko
import os
from pathlib import Path

def deploy_file(host, port, user, password, local_path, remote_path):
    """Deploy file using SFTP"""
    
    print(f"🔐 Connecting to {user}@{host}:{port}...")
    
    try:
        # Create SSH client
        ssh = paramiko.SSHClient()
        ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
        
        # Connect
        ssh.connect(host, port=port, username=user, password=password, timeout=10)
        print("✅ Connected!")
        
        # Open SFTP
        sftp = ssh.open_sftp()
        
        # Upload file
        print(f"📤 Uploading {local_path} → {remote_path}...")
        sftp.put(local_path, remote_path)
        print("✅ Upload successful!")
        
        # Verify
        stat = sftp.stat(remote_path)
        print(f"✅ File verified: {stat.st_size} bytes")
        
        # Close
        sftp.close()
        ssh.close()
        
        return True
        
    except Exception as e:
        print(f"❌ Error: {e}")
        return False

if __name__ == "__main__":
    # Configuration
    local_file = "src/MIMM.Frontend/wwwroot/js/mood-selector.js"
    remote_file = "/var/www/mimm-frontend/js/mood-selector.js"
    
    if not Path(local_file).exists():
        print(f"❌ {local_file} not found!")
        exit(1)
    
    # Check if paramiko is installed
    try:
        import paramiko
    except ImportError:
        print("❌ paramiko not installed!")
        print("   Install: pip install paramiko")
        exit(1)
    
    # Deploy
    success = deploy_file(
        host="188.245.68.164",
        port=2222,
        user="mimm",
        password="MIMMpassword",
        local_path=local_file,
        remote_path=remote_file
    )
    
    exit(0 if success else 1)
