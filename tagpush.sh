#!/bin/bash

tag=$1;

docker tag andylippitt/daud-client:dev andylippitt/daud-client:${tag}
docker tag andylippitt/daud-server:dev andylippitt/daud-server:${tag}
docker tag andylippitt/daud-static:dev andylippitt/daud-static:${tag}
docker push andylippitt/daud-client:${tag}
docker push andylippitt/daud-server:${tag}
docker push andylippitt/daud-static:${tag}
