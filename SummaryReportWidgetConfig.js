/*
 * Copyright 2016 Esri
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
      if (this.dataSourceConfig && this.dataSourceConfig.dataSourceId !== dataSourceConfig.dataSourceId) {
        this.configListDiv.innerHTML = "";
        this.configListDivOptions.innerHTML = "";
      }

      this.dataSourceConfig = dataSourceConfig;

      //setup fields config UI
      this._createTableOptions(dataSourceConfig.displayAll, dataSourceConfig.displayAlias);
      this.dataSourceConfig.selectedFieldsNames = this._createSimpleTable(dataSource);
      this._initTextBoxes(this.configListDiv.childNodes[0].rows, this.dataSourceConfig.displayAlias); 

      //setup note fields config UI
      this._createNoteFields(dataSourceConfig);
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
        var opCell = row.cells[2].childNodes[0];
        if (v && !domClass.contains(opCell, "configLabelDisabled")) {
          if (!domClass.contains(opCell, "configHeader")) {
            domClass.add(opCell, "configLabelDisabled");
          }
        } else {
          domClass.remove(opCell, "configLabelDisabled");
        }  
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

      //clone the datasource fields so we can compare and determine if any changes, such as...
      //adding or removing a field, has happened since the config was saved
      var dsFields = lang.clone(dataSource.fields);

      //remove any fields that are of a type other than the list below
      fieldLoop:
        for (var k in dsFields) {
          var f = dsFields[k];
          if (f.type !== "esriFieldTypeString" &&
              f.type !== "esriFieldTypeSmallInteger" &&
              f.type !== "esriFieldTypeInteger" &&
              f.type !== "esriFieldTypeSingle" &&
              f.type !== "esriFieldTypeDouble") {
            dsFields.splice(i, 1);
          }
        }

      var currentItems = [];
      if (this.dataSourceConfig.selectedFieldsNames) {
        for (var key in this.dataSourceConfig.selectedFieldsNames) {
          var persistField = this.dataSourceConfig.selectedFieldsNames[key];

          row = table.insertRow(idx);
          row.myIndex = idx;

          //TODO this approach would not handle field name changes
          var checked = persistField.checked;
          var displayName = persistField.displayName;
          var name = persistField.name;

          var fieldExists = false;
          configFieldLoop:
            for (var i = 0; i < dsFields.length; i++) {
              if (dsFields[i].name === name) {
                //remove the field from the datasource field list
                dsFields.splice(i, 1);
                //add the field to the persisted list
                if (checked) {
                  currentItems.splice(idx, 0, {
                    checked: checked,
                    displayName: displayName,
                    name: name,
                    indexInTable: idx
                  });
                }
                fieldExists = true;
                break configFieldLoop;
              }
            }

          if (fieldExists) {
            this._insertCell(row, checked, 0);
            this._insertCell(row, displayName, 1);
            this._insertCell(row, name, 2);
            this._insertCell(row, idx, 3);
            idx += 1;
          } else {
            //TODO test to verify this...this should be the case if a config was saved then a field 
            // was removed from the datasource
            console.log("Field: " + name + " does not exist in the datasource");
          }
        }
        //handle any remaining fields...this would be the case if a config was saved then
        // a field was added 
        this._addFields(table, dsFields, idx);
      } else {
        this._addFields(table, dsFields, idx);
      }
      return currentItems;
    },

    _addFields: function (table, fields, idx) {
      for (var k in fields) {
        var f = fields[k];

        row = table.insertRow(idx);
        row.myIndex = idx;

        var alias = f.alias;
        if (alias === "" || alias === null || typeof (alias) === 'undefined') {
          alias = f.name;
        }

        this._insertCell(row, false, 0);
        this._insertCell(row, alias, 1);
        this._insertCell(row, f.name, 2);
        this._insertCell(row, idx, 3);

        idx += 1;
      }
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
        var l = domConstruct.create('div', {
          className: "configOrderContainer"
        }, cell);

        domConstruct.create('div', {
          title: "Move Up",
          className: "configBaseOrderImage defaultUp configUpOrder",
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
          className: "configBaseOrderImage defaultDown configDownOrder",
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
      if (row.cells[0].childNodes[0].checked) {
        persistedNames.splice(index, 0, {
          checked: row.cells[0].childNodes[0].checked,
          displayName: row.cells[1].childNodes[0].value,
          name: fieldName,
          indexInTable: index
        });
      }

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
    },

    _createNoteFields: function (dataSourceConfig) {
      if (typeof (dataSourceConfig.noteFields) === 'undefined') {
        this.noteFields = [];
        dataSourceConfig.noteFields = [];
      } else {
        this.noteFields = dataSourceConfig.noteFields;
      }
      this.noteFields.push("Note Field Name");

      var table1 = domConstruct.create('table', {
        className: "tableTestNotes"
      }, this.configNotesListDiv);

      var table = domConstruct.create('tbody', {
        className: "tableTestNotes"
      }, table1);

      console.log(this.noteFields.length);
      for (var i = 0; i < this.noteFields.length; i++) {
        var row = table.insertRow(i);
        this._insertNoteFieldCell(row, this.noteFields[i], 0);
        this._insertNoteFieldCell(row, this.noteFields[i] !== "Note Field Name", 1);
        this._insertNoteFieldCell(row, "Remove", 2);
      }
    },

    _insertNoteFieldCell: function (row, v, idx) {
      var cell = row.insertCell(idx);
      if (idx === 0) {
        domConstruct.create('input', {
          value: v,
          className: "configTextBox",
          onclick: function (e) {
            this.select();
          },
          oninput: lang.hitch(this, function (e) {
            var row = e.srcElement.parentElement.parentElement;
            var noteFieldName = row.cells[0].childNodes[0].value;

            //persist with the config
            this.dataSourceConfig.noteFields[row.sectionRowIndex] = noteFieldName;
            //enable the remove checkbox
            row.cells[1].childNodes[0].disabled = false;
            this.readyToPersistConfig(this.dataSourceConfig.noteFields.length > 0);
            if (row.sectionRowIndex === row.offsetParent.rows.length - 1) {
              //add a new Note Field placeholder
              var newRow = row.offsetParent.insertRow(row.offsetParent.rows.length);

              this._insertNoteFieldCell(newRow, "Note Field Name", 0);
              this._insertNoteFieldCell(newRow, false, 1);
              this._insertNoteFieldCell(newRow, "Remove", 2);
            }
          })
        }, cell);
      } else if (idx === 1) {
        domConstruct.create('input', {
          type: "checkbox",
          checked: false,
          disabled: !v,
          onclick: lang.hitch(this, function (b) {
            var row = b.target.parentElement.parentElement;
            var table = row.offsetParent;
            this.dataSourceConfig.noteFields.splice(row.sectionRowIndex, 1);
            table.deleteRow(row.sectionRowIndex);
            this.readyToPersistConfig(this.dataSourceConfig.noteFields.length > 0);
          })
        }, cell);
      } else if (idx === 2) {
        var l = domConstruct.create('label', {
          innerHTML: v
        }, cell);
      }
    }
  });
});
