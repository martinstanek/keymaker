## The Keymaker

*... certificates and stuff ... work in progress*

[![Build status](https://awitec.visualstudio.com/Awitec/_apis/build/status/awitec.keymaker)](https://awitec.visualstudio.com/Awitec/_build/latest?definitionId=56)

![logo](https://github.com/martinstanek/keymaker/blob/develop/misc/logo.svg?raw=true)

![ui](https://github.com/martinstanek/keymaker/blob/develop/misc/ui.png?raw=true)

The Let's Encrypt client as a Docker image.\
Supports CloudFlare & Azure for the DNS challenge and Azure KeyVault as a target.

### Compose

```yml
services:

  keymaker.awitec.net:
    hostname: keymaker.awitec.net
    container_name: keymaker.awitec.net
    image: registry.lan.awitec.net/keymaker:0.0.109-amd64
    environment:
      # keymaker
      - KEYMAKER_DNSMODE=CloudFlare
      - KEYMAKER_STORAGEMODE=Volume
      - KEYMAKER_CHALLENGEMODE=Dns
      - KEYMAKER_AUTORENEW=true
      - KEYMAKER_RENEWEVERYHOURS=240
      - KEYMAKER_CHECKEVERYMINUTES=15
      - KEYMAKER_FOLDER=/certificates
      - KEYMAKER_WEBHOOK=true
      - KEYMAKER_WEBHOOKURL=http://10.0.1.243:5678/webhook/newCertificate
      - KEYMAKER_ENABLEUI=true
      - KEYMAKER_ENABLEAPI=true
      - KEYMAKER_ENABLECONSOLE=true
      - KEYMAKER_ENABLEOPENAPI=true
      - KEYMAKER_ENABLECHALLENGETRIGGER=true
      # certificate
      - KEYMAKER_CONTACT=info@example.com
      - KEYMAKER_DOMAIN=*.lan.example.com
      - KEYMAKER_CERTNAME=lan.example.com
      - KEYMAKER_PASSWORD=secret
      - KEYMAKER_COUNTRY=USA
      - KEYMAKER_STATE=Virginia
      - KEYMAKER_LOCALITY=Norfolk
      - KEYMAKER_ORG=Awitec
      - KEYMAKER_UNIT=HQ
      # cloud flare dns
      - KEYMAKER_CFDNSAPIEMAIL=youremail@example.com
      - KEYMAKER_CFDNSAPIKEY=1234
      - KEYMAKER_CFDNSAPIZONE=1234
      - KEYMAKER_CFDNSCHECKDOMAIN=_acme-challenge.lan.example.com
      - KEYMAKER_CFDNSSETDOMAIN=_acme-challenge.lan
    ports:
      - '6001:80'
    networks:
      - services
    volumes:
      - ./certificates:/certificates
      - /etc/localtime:/etc/localtime:ro
    restart: unless-stopped
```

![teaser](https://github.com/martinstanek/keymaker/blob/develop/misc/teaser.jpg?raw=true)