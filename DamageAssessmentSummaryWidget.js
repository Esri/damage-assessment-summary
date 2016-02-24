/*
 * Copyright 2015 Esri
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *   http://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
define([
  "dojo/_base/declare",
  "dojo/_base/lang",
  "dojo/has",
  "dojo/dom-construct",
  "dgrid/OnDemandList",
  "dgrid/Selection",
  "dijit/_WidgetBase",
  "dijit/_TemplatedMixin",
  "dijit/form/Button",
  "dojo/dom-class",
  "esri/opsdashboard/WidgetProxy",
  "esri/geometry/Extent",
  "dojo/store/Memory",
  "dojo/store/Observable",
  "esri/tasks/query",
  "dgrid/OnDemandGrid",
  "dojo/text!./DamageAssessmentSummaryWidgetTemplate.html"
], function (declare, lang, has, domConstruct, List, Selection, _WidgetBase, _TemplatedMixin, Button, domClass, WidgetProxy, Extent, Memory, Observable, Query, Grid, templateString) {

  return declare("DamageAssessmentSummaryWidget", [_WidgetBase, _TemplatedMixin, WidgetProxy], {
    templateString: templateString,
    debugName: "DamageAssessmentSummaryWidget",


    //TODO
    // handle zoom to point...right now it just pans
    // provide icon for row header to show open/close and highlight on hover
    // handle user entered values into the txt boxes or change them to labels
    // handle the issue that causes the row to collapse when you zoom or pan to a feature...also check this in the native app side
    // figure out how to get the style details for when the user changes themes...can't...Jay C logged a bug

    hostReady: function () {
      // Create the store we will use to display the features in the grid
      this.store = new Observable(new Memory());

      // Get from the data source and the associated data source config
      // The dataSourceConfig stores the fields selected by the operation view publisher during configuration
      var dataSourceProxy = this.dataSourceProxies[0];
      var dataSourceConfig = this.getDataSourceConfig(dataSourceProxy);

      this.getMapWidgetProxies().then(lang.hitch(this, function (results) {
        this._initQuery(dataSourceConfig, dataSourceProxy);
        this._createList(dataSourceConfig, dataSourceProxy, results[0]);
      }));
    },

    _initQuery: function (dataSourceConfig, dataSourceProxy) {
      var configDetails = dataSourceConfig.selectedFieldsNames;
      var oidName = dataSourceProxy.objectIdFieldName;
      var displayAlias = dataSourceConfig.displayAlias;
      console.log(displayAlias);

      //stores actual field name
      this.fieldsToQuery = [];
      //lookup object {fieldName: displayName}
      this.configFields = {};
      var idx = 0;
      for (var i = 0; i < configDetails.length; i++) {
        var f = configDetails[i];
        if (f.checked) {
          this.fieldsToQuery.splice(idx, 0, f.name);
          this.configFields[f.name] = displayAlias ? f.displayName : f.name;
          idx += 1;
        }
      }

      if (idx !== 0) {
        idx += 1;
      }

      if (this.fieldsToQuery.indexOf(oidName) === -1) {
        this.fieldsToQuery.splice(idx, 0, oidName);
        //this.configFields[oidName] = oidName;
      }

      this.query = new Query();
      this.query.outFields = this.fieldsToQuery;
      console.log(this.fieldsToQuery);
      this.query.returnGeometry = true;
    },

    _createList: function (dataSourceConfig, dataSourceProxy, mapProxy) {
      this.list = new (declare([List, Selection]))({
        mapProxy: mapProxy,
        dataProxy: dataSourceProxy,
        store: this.store,
        fields: this.fieldsToQuery,
        configFields: this.configFields,
        cleanEmptyObservers: false,
        selectionMode: this.isNative ? "extended" : "toggle",
        renderRow: function (feature) {
          var divNode = domConstruct.create('div', {
            className: "bottomBorder"
          });

          var titleDiv = domConstruct.create('div', {
            style: "height: 30px; width: 100%;",
            onclick: function (evt) {
              var t = evt.target.nextSibling;
              var fc = t.firstChild;
              if (domClass.contains(t, "rowOff")) {
                domClass.remove(t, "rowOff");
                domClass.add(t, "rowOn");
                //domClass.remove(fc, "downImage");
                //domClass.add(fc, "upImage");
              } else {
                domClass.remove(t, "rowOn");
                domClass.add(t, "rowOff");
                //domClass.remove(fc, "upImage");
                //domClass.add(fc, "downImage");
              }
            }
          }, divNode);

          domConstruct.create('div', {
            className: "downImage"
          }, titleDiv);

          var contentDiv = domConstruct.create('div', {
            className: "rowOff",
            id: feature.id
          }, divNode);

          var idx = 0;
          for (var i = 0; i < this.fields.length; i++) {
            var fieldName = this.fields[i];
            //do this so we can have the OID in the query to support selection
            // but avoid drawing in the widget
            if (typeof (this.configFields[fieldName]) !== 'undefined') {
              domConstruct.create('label', {
                className: "fieldItemLabel",
                innerHTML: this.configFields[fieldName] + ":"
              }, contentDiv);
              domConstruct.create('input', {
                className: "fieldItemValue",
                value: feature.attributes[fieldName]
              }, contentDiv);

              if (idx === 0) {
                titleDiv.innerHTML = feature.attributes[fieldName];
                idx += 1;
              }
            }
          }

          var alignContainer = domConstruct.create('div', {
            className: "fieldItemLabel"
          }, contentDiv);

          var btnContainer = domConstruct.create('div', {
            className: "btnParent"
          }, alignContainer);

          domConstruct.create('button', {
            className: "my-btn",
            innerHTML: "Zoom To Feature",
            onclick: lang.hitch(this, function (evt) {
              var row = evt.target.parentElement.parentElement.parentElement;
              var rowData = this.row(row.id).data;
              if (rowData) {
                var geom = rowData.geometry;
                if (geom.type === "point") {
                  this.mapProxy.panTo(geom);
                } else {
                  this.mapProxy.setExtent(geom);
                }
              }
            })
          }, btnContainer);

          if (this.dataProxy.supportsSelection) {
            domConstruct.create('button', {
              className: "my-btn",
              innerHTML: "Select Feature",
              onclick: lang.hitch(this, function (evt) {
                var row = evt.target.parentElement.parentElement.parentElement;
                this.dataProxy.selectFeatures(row.id);
              })
            }, btnContainer);
          }

          return divNode;
        }
      }, this.listDiv);

      this.list.startup();
    },

    dataSourceExpired: function (dataSourceProxy, dataSourceConfig) {

      //TODO extend this like the list example to account for feature actions??

      // Execute the query. A request will be sent to the server to query for the features.
      // The results are in the featureSet
      dataSourceProxy.executeQuery(this.query).then(function (featureSet) {
        //// Show the features in the table
        //this.updateAttributeTable(featureSet, dataSourceProxy);

        if (this.store.data.length > 0) {
          this.store.query().forEach(function (item) {
            this.store.remove(item.id);
          }.bind(this));
        }

        if (featureSet.features) {
          featureSet.features.forEach(function (feature) {
            this.store.put(feature, {
              overwrite: true,
              id: feature.attributes[dataSourceProxy.objectIdFieldName]
            });
          }.bind(this));
        }
      }.bind(this));
    },

    _exportCSV: function () {
      if (this.store.data.length > 0) {
        var csvData = "";
        var attributes;
        var attribute;
        var line = "";
        var hasColumnNames = false;
        this.store.query().forEach(function (item) {
          attributes = item.attributes ? item.attributes : item;

          // build the header
          if (!hasColumnNames) {
            for (attribute in attributes) {
              //TODO need to filter out the OID field
              csvData += (csvData.length === 0 ? "" : ",") + '"' + attribute + '"';
            }
            csvData += "\r\n";
            hasColumnNames = true;
          }

          //populate the columns
          line = "";
          for (attribute in attributes) {
            line += (line.length === 0 ? "" : ",") + '"' + attributes[attribute] + '"';
          }
          csvData += line + "\r\n";
        });
        //fileName - for download
        //TODO should I get like this or store at an earlier point??
        var filename = this.dataSourceProxies[0].name + ".csv";

        // native - open the data in excel file straight
        if (this.isNative) {
          csvData = "sep=,\r\n" + csvData;
          window.open("data:application/vnd.ms-excel;charset=utf-8," + encodeURIComponent(csvData), filename);
          return;
        }
        //web - the file will be downloaded to the default download location
        // Handle chrome
        if (has("chrome")) {
          var link = domConstruct.create("a", {
            href: 'data:attachment/csv;charset=utf-8,' + encodeURIComponent(csvData),
            download: filename
          }, window.document.body);

          var clickEvent = window.document.createEvent("MouseEvents");
          clickEvent.initEvent("click", true /* bubble */, true /* cancelable */);
          link.dispatchEvent(clickEvent);
          return;
        }

        //handle Firefox
        if (has("ff")) {
          var link = domConstruct.create("a", {
            href: 'data:attachment/csv;charset=utf-8,' + encodeURIComponent(csvData),
            download: filename
          }, window.parent.document.body);

          var clickEvent = window.document.createEvent("MouseEvents");
          clickEvent.initEvent("click", true /* bubble */, true /* cancelable */);
          link.dispatchEvent(clickEvent);
          return;
        }

        //handle Microsoft Internet Explorer
        var blob = new Blob([csvData], { "type": "text/csv;charset=utf8" });
        window.navigator.msSaveOrOpenBlob(blob, filename);
      }
      return;
    }

  });
});






















