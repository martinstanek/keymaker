## The Keymaker

*... certificates and stuff ... work in progress*

[![Build status](https://awitec.visualstudio.com/Awitec/_apis/build/status/awitec.keymaker)](https://awitec.visualstudio.com/Awitec/_build/latest?definitionId=56)
![Docker Image Version](https://img.shields.io/docker/v/awitec/keymaker)

![logo](https://github.com/martinstanek/keymaker/blob/develop/misc/logo.svg?raw=true)

![ui](https://github.com/martinstanek/keymaker/blob/develop/misc/ui.png?raw=true)

The Let's Encrypt client is a Docker image.\
Supports HTTP & DNS challenges.\
Supports CloudFlare & Azure DNS Zones.\
Supports local volume & Azure KeyVault as a target for the certificate persistence.\
Supports extended automation via the webhook.

### Server

```text
http://localhost - UI
http://localhost/health - Health check
http://localhost/console - Raw log console
http://localhost/swagger - Swagger
http://localhost/.well-known/acme-challenge - ACME HTTP challenge endpoint
```

### Webhook

The Kaymaker can trigger an external automation workflow by calling a webhook.\
The webhook (if configured) is triggered after the successful certificate acquisition.

The HTTP POST request with the following payload will be fired:

```json
{
  "domain":"your.domain.com",
  "issuer":"Let's Encrypt",
  "fullChainPem":"..",
  "privateKeyPem":"..",
  "base64FullChainPem": "..",
  "base64PrivateKeyPem": "..",
  "base64Pfx": "..",
  "password":"..",
  "obtained":"2025-12-05T16:23:55.8774133+00:00",
  "expiry":"2026-03-05T15:25:23+00:00"
}
```
> **_NOTE:_**  Be careful when passing your certificates to another system.

### Compose

```yml
services:

  keymaker.awitec.net:
    hostname: keymaker.awitec.net
    container_name: keymaker.awitec.net
    image: awitec/keymaker:1.0.0-amd64
    environment:
      # general config
      - KEYMAKER_DNSMODE=CloudFlare # CloudFlare or Azure
      - KEYMAKER_STORAGEMODE=KeyVault # KeyVault or Volume
      - KEYMAKER_CHALLENGEMODE=Dns # Dns or Http
      - KEYMAKER_AUTORENEW=true # true or false for auto-renewal
      - KEYMAKER_RENEWEVERYHOURS=240 # acquire new certificate every x hours 
      - KEYMAKER_CHECKEVERYMINUTES=15 # check if certificate should be renewed every y minutes
      - KEYMAKER_FOLDER=/certificates # if volume as store confirmed, this is top level folder
      - KEYMAKER_WEBHOOK=true # true or false for the webhook
      - KEYMAKER_WEBHOOKURL=http://10.0.1.243:5678/webhook/newCertificate # example of webhook url
      - KEYMAKER_ENABLEUI=true # true or false for the UI
      - KEYMAKER_ENABLEAPI=true # true or false for the API (without the API of course the UI would not work)
      - KEYMAKER_ENABLECONSOLE=true # true or false for console
      - KEYMAKER_ENABLEOPENAPI=true # true or false for Swagger
      - KEYMAKER_ENABLECHALLENGETRIGGER=true # true or false for the trigger via the API/UI
      # certificate parameters
      - KEYMAKER_CONTACT=info@example.com # contact (email)
      - KEYMAKER_DOMAIN=*.lan.example.com # requested domain
      - KEYMAKER_CERTNAME=lan.example.com # name of the certificate
      - KEYMAKER_PASSWORD=secret # password for the certificate
      - KEYMAKER_COUNTRY=USA 
      - KEYMAKER_STATE=Virginia
      - KEYMAKER_LOCALITY=Norfolk
      - KEYMAKER_ORG=Awitec
      - KEYMAKER_UNIT=HQ
      # cloud flare dns (required if CloudFlare is set as DNS mode)
      - KEYMAKER_CFDNSAPIEMAIL=youremail@example.com # an account email
      - KEYMAKER_CFDNSAPIKEY=1234 # the API key (check CF documentation)
      - KEYMAKER_CFDNSAPIZONE=1234 # the zone ID (check CF documentation)
      - KEYMAKER_CFDNSCHECKDOMAIN=_acme-challenge.lan.example.com
      - KEYMAKER_CFDNSSETDOMAIN=_acme-challenge.lan
      # azure (required if Azure DNS or Azure KeyVault used)
      - KEYMAKER_AZCLIENTID=000-000-000
      - KEYMAKER_AZTENANTID=000-000-000
      - KEYMAKER_AZSECRET=secret
      # azure dns (required if Azure is set as DNS mode)
      - KEYMAKER_AZDNSRESOURCEID=/subscriptions/000/resourceGroups/your-rg/providers/Microsoft.Network/dnszones/dns-zone-name
      - KEYMAKER_AZDNSCHECKDOMAIN=_acme-challenge.example.com
      - KEYMAKER_AZDNSSETDOMAIN=_acme-challenge
      # azure kv (required if KeyVault is set as storage mode)
      - KEYMAKER_AZKVURL=https://some-vault.vault.azure.net/
      - KEYMAKER_AZKVCERTIFICATENAME=example-com
    ports:
      - '6001:80'
    networks:
      - services
    volumes:
      - ./certificates:/certificates
      - /etc/localtime:/etc/localtime:ro
    restart: unless-stopped
```

Since quite sensitive info has to be provided to the container,\
those values can be passed in as docker secrets.

The simplified compose file with the full list of env variable variants\
supporting the docker secrets:

```yml

services:

  keymaker.awitec.net:
    hostname: keymaker.awitec.net
    container_name: keymaker.awitec.net
    image: awitec/keymaker:1.0.0-amd64
    environment:
      - KEYMAKER_CFDNSAPIEMAIL_FILE=/run/secrets/cf_email
      - KEYMAKER_CFDNSAPIKEY_FILE=/run/secrets/cf_key
      - KEYMAKER_CFDNSAPIZONE_FILE=/run/secrets/cf_zone
      - KEYMAKER_AZSECRET_FILE=/run/secrets/az_secret
      - KEYMAKER_CONTACT_FILE=/run/secrets/cert_contact
      - KEYMAKER_PASSWORD_FILE=/run/secrets/cert_pass
    secrets:
      - cert_pass
      - cert_contact
      - cf_key
      - cf_email
      - cf_zone
      - az_secret
    
secrets:
  cert_pass:
    file: ./secrets/cert_pass.txt
  cert_contact:
    file: ./secrets/cert_contact.txt
  cf_key:
    file: ./secrets/cf_apikey.txt
  cf_email:
    file: ./secrets/cf_email.txt
  cf_zone:
    file: ./secrets/cf_zone.txt
  az_secret:
    file: ./secrets/az_secret.txt
```

![teaser](https://github.com/martinstanek/keymaker/blob/develop/misc/teaser.jpg?raw=true)