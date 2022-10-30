Push-Location -Path infrastructure 
& ./stop.ps1
Pop-Location

docker compose down 