define(['backbone',
    'app/views/endpoints',
    'app/views/auditMessages',
    'app/views/errorMessages',
    'app/views/endpointDetails',
    'app/views/auditMessageDetails',
    'app/views/errorMessageDetails',
    'app/views/navigation',
    'app/views/settings',
    'app/views/route',
    'app/collections/heartbeats',
    'app/models/endpoint',
    'app/models/audit',
    'app/models/error',
    'app/models/settings',
    'moment'
], function(Backbone, EndpointsView, AuditsView, ErrorsView, EndpointDetailsView,
    AuditDetailsView, ErrorDetailsView, Navigation, SettingsView, RouteView,
    HeartbeatCollection, EndpointModel, AuditModel, ErrorModel, SettingsModel,
    moment) {

    "use strict";

    var router = Backbone.Router.extend({

        routes: {
            "": "endpoints",
            "endpoints(/)": "endpoints",
            "endpoint/:name/:location": "endpointDetails",
            "audit(/)": "audits",
            "audit/:id": "auditDetails",
            "error(/)": "errors",
            "error/:id": "errorDetails",
            "settings(/)": "settings",
            "route/:id": "messageRoute"
        },

        before: function(route) {
            if (!this.navigation) {
                this.navigation = new Navigation();
                this.navigation.render();
            }
            if (route === '') {
                route = 'endpoints';
            }
            this.navigation.setActive(route);

            this.closeViews();
        },

        endpoints: function() {
            var view = new EndpointsView();
            this.renderView(view);
        },

        audits: function() {
            var view = new AuditsView();
            this.renderView(view);
        },

        errors: function() {
            var view = new ErrorsView();
            this.renderView(view);
        },

        endpointDetails: function(name, location) {
            var that = this;
            var collection = new HeartbeatCollection();
            var model = new EndpointModel();
            var modelPromise = model.fetch({
                data: {
                    name: name,
                    location: location
                }
            });
            var collectionPromise = collection.fetch({
                data: {
                    name: name,
                    location: location,
                    from: moment.utc().subtract(1, "hours").format(),
                    to: moment.utc().format()
                }
            });
            $.when(modelPromise, collectionPromise).done(function() {
                model.set({
                    From: moment.utc().subtract(1, "hours").format(),
                    To: moment.utc().format()
                });
                var view = new EndpointDetailsView({
                    collection: collection,
                    model: model
                });
                that.renderView(view);
            });
        },

        auditDetails: function(id) {
            var that = this;
            var model = new AuditModel({
                Id: id
            });
            model.fetch({
                success: function() {
                    var view = new AuditDetailsView({
                        model: model
                    });
                    that.renderView(view);
                }
            });
        },

        errorDetails: function(id) {
            var that = this;
            var model = new ErrorModel({
                Id: id
            });
            model.fetch({
                success: function() {
                    var view = new ErrorDetailsView({
                        model: model
                    });
                    that.renderView(view);
                }
            });
        },

        settings: function() {
            var that = this;
            var model = new SettingsModel();
            model.fetch({
                success: function() {
                    var view = new SettingsView({
                        model: model
                    });
                    that.renderView(view);
                }
            });
        },

        messageRoute: function(id) {
            var view = new RouteView({
                correlationId: id
            });
            this.renderView(view);
        }
    });

    return router;
});
