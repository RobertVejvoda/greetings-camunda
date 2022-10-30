Push-Location -Path infrastructure 
& ./start.ps1
Pop-Location

docker compose up -d --build