SET BASEURL=https://raw.githubusercontent.com/daud-io/daud/master/Samples/Weather

call game world hook default --url %BASEURL%/fish-extra.json
sleep 5
call game world hook default --url %BASEURL%/fish-standard.json
sleep 5