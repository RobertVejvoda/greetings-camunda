Push-Location -Path ./mosquitto 
& ./build.ps1
Pop-Location

docker network create --driver bridge camunda-cloud

docker compose up -d