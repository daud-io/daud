#!/bin/bash

docker build . -t andylippitt/daud:$1
docker push andylippitt/daud:$1

