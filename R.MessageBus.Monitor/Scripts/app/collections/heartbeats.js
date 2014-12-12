define(['backbone', 'backbone-pageable'], function(Backbone) {

    "use strict";

    var collection = Backbone.PageableCollection.extend({
        url: "heartbeats",
        mode: "client"
    });

    return collection;
});