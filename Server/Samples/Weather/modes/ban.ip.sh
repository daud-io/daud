#!/bin/bash
iptables -I DOCKER-USER -i ens3 -s $1 -j DROP
