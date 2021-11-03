#!/bin/bash

apply()
{
	game world hook default --file $1.json
}

apply fish-fast
sleep 5m
apply fish-extra
sleep 5m
apply fish-standard
sleep 1s

apply dauds-extra
sleep 5m
apply dauds-standard
sleep 1

apply shields-none
sleep 5m
apply shields-standard
sleep 5m
apply shields-extra
sleep 5m
apply shields-standard
sleep 5m

apply obstacles-none
sleep 1
apply obstacles-standard
apply obstacles-fast
sleep 5m
apply obstacles-extra
sleep 5m
apply obstacles-standard
sleep 5m

apply robotguns-extra
sleep 5m
apply robotguns-standard
sleep 5m
