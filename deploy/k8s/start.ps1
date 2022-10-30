kubectl apply `
    -f namespace.yaml `
    -f secrets.yaml `
    -f zipkin.yaml `
    -f mosquitto.yaml `
    -f maildev.yaml `
    -f dapr-config.yaml `
    -f email.yaml `
    -f greetings-api.yaml

