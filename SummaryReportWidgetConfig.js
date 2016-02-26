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
  'dojo/dom-construct',
  "dojo/store/Memory",
  'dojo/dom-class',
  'dojo/_base/html',
  "dijit/form/CheckBox",
  "dgrid/OnDemandList",
  "dgrid/Selection",
  "dijit/_WidgetBase",
  "dijit/_TemplatedMixin",
  "dijit/_WidgetsInTemplateMixin",
  "dojox/form/CheckedMultiSelect",
  "esri/opsdashboard/WidgetConfigurationProxy",
  "dojo/text!./SummaryReportWidgetConfigTemplate.html",
  "dojo/parser"
], function (declare, lang, domConstruct, Memory, domClass, html, CheckBox, List, Selection, _WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, CheckedMultiSelect, WidgetConfigurationProxy, templateString) {

  return declare("SummaryReportWidgetConfig", [_WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, WidgetConfigurationProxy], {
    templateString: templateString,

    postCreate: function () {
      this.inherited(arguments);
    },

    dataSourceSelectionChanged: function (dataSource, dataSourceConfig) {
      this.dataSourceConfig = dataSourceConfig;

      //TODO need to understand when and what all happens to the config when a datasource changes
      // also need to understand when it's ok to check the config values
      this._createTableOptions(dataSourceConfig.displayAll, dataSourceConfig.displayAlias);
      this.dataSourceConfig.selectedFieldsNames = this._createSimpleTable(dataSource);

      console.log("Selected Names: ");
      console.log(this.dataSourceConfig.selectedFieldsNames);
      this._initTextBoxes(this.configListDiv.childNodes[0].rows, this.dataSourceConfig.displayAlias); 
    },

    _createTableOptions: function (displayAll, displayAlias) {
      var dAll = typeof (displayAll) !== 'undefined' ? displayAll : false;
      this.dataSourceConfig.displayAll = dAll;

      var dAlias = typeof (displayAlias) !== 'undefined' ? displayAlias : true;
      this.dataSourceConfig.displayAlias = dAlias;

      //Display All Option
      domConstruct.create('input', {
        type: "checkbox",
        id: "displayAll",
        checked: dAll,
        className: "optionsCheckbox",
        onclick: lang.hitch(this, function (v) {
          this.dataSourceConfig.displayAll = v.target.checked;
          var rows = this.configListDiv.childNodes[0].rows;
          for (var i = 0; i < rows.length; i++) {
            var row = rows[i];
            var cbCell = row.cells[0];
            if (v.target.checked && !cbCell.childNodes[0].checked) {
              cbCell.childNodes[0].click();
            } else if (!v.target.checked && cbCell.childNodes[0].checked) {
              cbCell.childNodes[0].click();
            }
          }
          this.readyToPersistConfig(true);
        })
      }, this.configListDivOptions);

      domConstruct.create('label', {
        innerHTML: "Display All Fields",
        className: "optionsLabel",
        "for": "displayAll"
      }, this.configListDivOptions);

      //Display Field Alias Option
      domConstruct.create('input', {
        type: "checkbox",
        id: "displayAlias",
        checked: dAlias,
        className: "optionsCheckbox",
        onclick: lang.hitch(this, function (v) {
          this.dataSourceConfig.displayAlias = v.target.checked;
          var rows = this.configListDiv.childNodes[0].rows;
          this._initTextBoxes(rows, v.target.checked);
          this.readyToPersistConfig(true);
        })
      }, this.configListDivOptions);

      domConstruct.create('label', {
        innerHTML: "Display Field Alias",
        className: "optionsLabel",
        "for": "displayAll"
      }, this.configListDivOptions);
    },

    _initTextBoxes: function (rows, v) {
      for (var i = 0; i < rows.length; i++) {
        var row = rows[i];
        row.cells[1].childNodes[0].disabled = !v;
      }
    },

    _createSimpleTable: function (dataSource) {
      var table1 = domConstruct.create('table', {
        className: "tableTest"
      }, this.configListDiv);

      var table = domConstruct.create('tbody', {
        className: "tableTest"
      }, table1);

      var idx = 0;
      var row;
      var header = table1.createTHead();
      var row = header.insertRow(0);

      this._insertHeaderCell(row, "Display", 0);
      this._insertHeaderCell(row, "Field Alias", 1);
      this._insertHeaderCell(row, "Field Name", 2);
      this._insertHeaderCell(row, "Order", 3);

      //I Guess I will load from the config if it's defiend
      //and remove from this copy if it's found...so I know what ones are left

      //console.log("starting field work..................");
      var dsFields = lang.clone(dataSource.fields);
      //console.log(dsFields);
      fieldLoop:
        for (var k in dsFields) {
          var f = dsFields[k];
          if (f.type !== "esriFieldTypeString" &&
              f.type !== "esriFieldTypeSmallInteger" &&
              f.type !== "esriFieldTypeInteger" &&
              f.type !== "esriFieldTypeSingle" &&
              f.type !== "esriFieldTypeDouble") {
            dsFields.splice(i, 1);
            //console.log("Removing field: " + f.name + " " + f.type);
          }
        }
      //console.log(dsFields);
      var currentItems = [];
      if (this.dataSourceConfig.selectedFieldsNames) {
        for (var key in this.dataSourceConfig.selectedFieldsNames) {
          var persistField = this.dataSourceConfig.selectedFieldsNames[key];
          //console.log(persistField);
          row = table.insertRow(idx);
          row.myIndex = idx;
          //console.log("Setting persisted index: " + idx);
          //console.log(row);

          var checked = persistField.checked;
          var displayName = persistField.displayName;
          var name = persistField.name;
          //console.log("Inserting persisted index: " + idx);
          currentItems.splice(idx, 0, {
            checked: checked,
            displayName: displayName,
            name: name,
            indexInTable: idx
          });

          configFieldLoop:
            for (var i = 0; i < dsFields.length; i++) {
              if (dsFields[i].name === name) {
                dsFields.splice(i, 1);
                //console.log("remove: " + name);
                break configFieldLoop;
              }
            }
          //console.log("inserting row: " + row);
          this._insertCell(row, checked, 0);
          this._insertCell(row, displayName, 1);
          this._insertCell(row, name, 2);
          this._insertCell(row, idx, 3);
          //console.log("inserted row: " + row);
          idx += 1;
        }
      } else {
        dataSource.fields.forEach(lang.hitch(this, function (field) {
          //console.log("Setting myIndex: " + idx);
          switch (field.type) {
            case "esriFieldTypeString":
            case "esriFieldTypeSmallInteger":
            case "esriFieldTypeInteger":
            case "esriFieldTypeSingle":
            case "esriFieldTypeDouble":
              row = table.insertRow(idx);
              row.myIndex = idx;
              var checked = false;
              var displayName = field.alias;
              //console.log("Inserting index: " + idx);
              currentItems.splice(idx, 0, {
                checked: checked,
                displayName: displayName,
                name: field.name,
                indexInTable: idx
              });
              //console.log("inserting row: " + row);
              this._insertCell(row, checked, 0);
              this._insertCell(row, displayName, 1);
              this._insertCell(row, field.name, 2);
              this._insertCell(row, idx, 3);
              //console.log("inserted row: " + row);
              idx += 1;
              //console.log("Incrementing index: " + idx);
              return;
          }
        }));
      }

      return currentItems;
    },

    _insertCell: function (row, v, idx) {
      var cell = row.insertCell(idx);
      if (idx === 0) {
        domConstruct.create('input', {
          type: "checkbox",
          checked: v,
          onclick: lang.hitch(this, function (b) {
            var row = b.target.parentElement.parentElement;
            var fieldName = row.cells[2].childNodes[0].textContent;
            this._updateList(row, fieldName, row.myIndex);
          })
        }, cell);
        html.setStyle(cell, 'text-align', 'center');
      } else if (idx === 1) {
        domConstruct.create('input', {
          value: v,
          className: "configTextBox",
          oninput: lang.hitch(this, function (e) {
            var row = e.srcElement.parentElement.parentElement;
            var fieldName = row.cells[2].childNodes[0].textContent;
            this._updateList(row, fieldName, row.myIndex)
          })
        }, cell);
      } else if (idx === 2) {
        var l = domConstruct.create('label', {
          innerHTML: v
        }, cell);
      } else if (idx === 3) {
        //TODO need up image, down image, and up/down image
        // set down only image for the first row and up only image for the last row and
        // up/down for the rest
        if (v === 0) {
          //set down only image on hover
        } 

        var l = domConstruct.create('div', {
          className: "configOrderContainer"
        }, cell);

        domConstruct.create('div', {
          title: "Move Up",
          className: "configBaseOrderImage configUpOrder",
          onclick: lang.hitch(this, function (b) {
            var row = b.target.offsetParent.parentElement;
            var table = row.parentNode;
            var rows = table.rows;
            var index = row.sectionRowIndex;
            var newIndex = index - 1;
            if (index > 0) {
              var refRow = rows[newIndex];
              table.insertBefore(row, refRow);
              row.myIndex = newIndex;
              refRow.myIndex = index;
              this._updateList(row, row.cells[2].textContent, row.myIndex);
              this._updateList(refRow, refRow.cells[2].textContent, refRow.myIndex);
            }
          })
        }, l);

        var l = domConstruct.create('div', {
          title: "Move Down",
          className: "configBaseOrderImage configDownOrder",
          onclick: lang.hitch(this, function (b) {
            var row = b.target.offsetParent.parentElement;
            var table = row.parentNode;
            var rows = table.rows;
            var index = row.sectionRowIndex;
            var newIndex = index + 1;
            if (index < rows.length) {
              var refRow = rows[newIndex];
              table.insertBefore(refRow, row);
              row.myIndex = newIndex;
              refRow.myIndex = index;
              this._updateList(row, row.cells[2].textContent, row.myIndex);
              this._updateList(refRow, refRow.cells[2].textContent, refRow.myIndex);
            }
          })
        }, l);
      }
    },

    _insertHeaderCell: function (row, v, idx) {
      var cell = row.insertCell(idx);
      domConstruct.create('label', {
        className: "configHeader",
        innerHTML: v
      }, cell);
    },

    _updateList: function (row, fieldName, index) {
      //update the persisted values for the config
      var persistedNames = this.dataSourceConfig.selectedFieldsNames;
      for (var i = 0; i < persistedNames.length; i++) {
        if (persistedNames[i].name === fieldName) {
          persistedNames.splice(i, 1);
          break;
        }
      }

      persistedNames.splice(index, 0, {
        checked: row.cells[0].childNodes[0].checked,
        displayName: row.cells[1].childNodes[0].value,
        name: fieldName,
        indexInTable: index
      });

      console.log("Persisted Names: ");
      console.log(persistedNames);

      this.readyToPersistConfig(Array.isArray(persistedNames) && persistedNames.length > 0);
    },

    onSelectionChanged: function (value) {
      //TODO this is out of sync and needs to be updated
      if (!this.dataSourceConfig)
        return;

      this.dataSourceConfig.selectedFieldsNames = value;
      this.readyToPersistConfig(Array.isArray(value) && value.length > 0);
    },

    rowClicked: function (evt) {
      var parent = evt.target.parentNode;
      if (!domClass.contains(parent, "bottomBorder")) {
        for (var i = 0; i < 2; i++) {
          parent = parent.parentNode;
          if (domClass.contains(parent, "bottomBorder")) {
            break;
          }
        }
      }
      var row = parent.childNodes[3];
      var img = parent.childNodes[1].childNodes[1];
      if (domClass.contains(row, "rowOff")) {
        domClass.remove(row, "rowOff");
        domClass.add(row, "rowOn");
        domClass.remove(img, "downImage");
        domClass.add(img, "upImage");
        domClass.remove(img, "image-down-highlight");
        domClass.add(img, "image-up-highlight");
      } else {
        domClass.remove(row, "rowOn");
        domClass.add(row, "rowOff");
        domClass.remove(img, "upImage");
        domClass.add(img, "downImage");
        domClass.remove(img, "image-up-highlight");
        domClass.add(img, "image-down-highlight");
      }
    }
  });
});
















