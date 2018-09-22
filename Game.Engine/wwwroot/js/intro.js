(function () {
    var max = 3;
    $.get("https://twitrss.me/twitter_user_to_rss/?user=IoDaud", function (data) {
        $(data).find("item").each(function () { // or "item" or whatever suits your feed
            var el = $(this);
            if (max-- == 0)
                return false;

            console.log("title      : " + el.find("title").text());
            console.log("description: " + el.find("description").html());
            console.log("date:" + new Date(el.find("pubDate").text()).toLocaleDateString());

            var html = el.find("description").html();
            var date = new Date(el.find("pubDate").text()).toLocaleDateString();

            html = html.replace('<![CDATA[', '');
            html = html.replace(']]>', '');

            var div = $("<div />");
            var dateSpan = $('<span class="date"></span>');
            dateSpan.append(date);
            div.append(dateSpan);
            div.append($(html));
            div.find('a').attr('target', '_top');


            $('#twitterpated').append(div);
        });
    });
})();
