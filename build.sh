#!/bin/bash

docker build . -t iodaud/daud:$1
docker push iodaud/daud:$1

