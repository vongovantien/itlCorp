import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChanges, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { AppPage } from 'src/app/app.base';

import cloneDeep from 'lodash/cloneDeep';
@Component({
    selector: 'app-combo-grid-virtual-scroll',
    templateUrl: './combo-grid-virtual-scroll.component.html'
})
export class ComboGridVirtualScrollComponent extends AppPage implements OnInit, OnChanges, AfterViewInit {

    @Input() dataSources: any[];
    @Input() displayFields: { field: string, label: string }[];
    @Input() searchFields: any[];
    @Input() selectedDisplayFields: any[];
    @Input() currentActiveItemId: any;
    @Input() disabled: boolean;
    @Input() height: number = 100;
    @Input() isTooltip: boolean = false;

    @Output() itemSelected = new EventEmitter<any>();

    currentItemSelected: any = null;
    CurrentActiveItemIdObj: { field: string, value: any, hardValue: any } = null;
    indexSelected: number = -1;
    displaySelectedStr: string = '';
    ConstDataSources: any[] = [];
    DataSources: any[] = [];
    DisplayFields: { field: string, label: string }[] = [];
    SearchFields: string[] = [];
    SelectedDisplayFields: string[] = [];
    Disabled: boolean = false;

    constructor(
        private cdr: ChangeDetectorRef,
    ) {
        super();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (!!changes.dataSources) {
            this.setDataSource(changes.dataSources.currentValue);
        }
        if (!!changes.displayFields) {
            this.setDisplayFields(changes.displayFields.currentValue);
        }
        if (!!changes.selectedDisplayFields) {
            this.setSelectedDisplayFields(changes.selectedDisplayFields.currentValue);
        }
        if (!!changes.currentActiveItemId) {
            this.setCurrentActiveItemId(changes.currentActiveItemId.currentValue);
        }
        if (changes.disabled !== undefined && changes.disabled !== null) {
            this.setDisabled(changes.disabled.currentValue);
        }
    }

    ngAfterViewInit(): void {
        this.cdr.markForCheck();
    }

    ngOnInit() {

    }

    setDataSource(data: any[]) {
        if (!!data && data.length > 0) {
            this.DataSources = data;
            this.ConstDataSources = cloneDeep(data);
            if (this.CurrentActiveItemIdObj !== null) {
                const activeItemData = this.CurrentActiveItemIdObj;
                const itemIndex = this.ConstDataSources.findIndex(o => o[activeItemData.field] === activeItemData.value);

                if (itemIndex !== -1) {
                    this.indexSelected = itemIndex;
                    const item = this.ConstDataSources[itemIndex];

                    this.setCurrentActiveItem(item);
                }
                if (itemIndex === -1 && activeItemData.hardValue != null) {
                    this.displaySelectedStr = activeItemData.hardValue;
                }
            }
        } else {
            this.DataSources = [];
        }
    }

    setDisplayFields(data: { field: string, label: string }[]) {
        if (data.length > 0) {
            this.DisplayFields = data;
        }
    }

    setSelectedDisplayFields(data: any[]) {
        if (data.length > 0) {
            this.SelectedDisplayFields = data;
        }
    }

    setDisabled(data: boolean) {
        this.Disabled = data;
    }

    setCurrentActiveItemId(data: any) {
        if (data.value != null) {
            this.CurrentActiveItemIdObj = data;

            const itemIndex = this.ConstDataSources.findIndex(i => i[data.field] === data.value);

            if (itemIndex !== -1) {
                this.indexSelected = itemIndex;
                const item = this.ConstDataSources[itemIndex];
                this.setCurrentActiveItem(item);
            }
            if (itemIndex === -1 && data.hardValue != null) {
                this.displaySelectedStr = data.hardValue;
            }
        } else {
            this.indexSelected = -1;
            this.displaySelectedStr = null;
        }
    }

    emitSelected(item: any) {
        this.itemSelected.emit(item);

        this.setCurrentActiveItem(item);

        this.currentItemSelected = item;
        if (this.CurrentActiveItemIdObj !== null && this.CurrentActiveItemIdObj.value !== null) {
            this.CurrentActiveItemIdObj.value = item[this.CurrentActiveItemIdObj.field];
        }
    }

    setCurrentActiveItem(item: any) {
        this.displaySelectedStr = '';
        for (let i = 0; i < this.SelectedDisplayFields.length; i++) {
            const field = this.SelectedDisplayFields[i];

            if (i === this.SelectedDisplayFields.length - 1) {
                this.displaySelectedStr += this.getValue(item, field);
            } else {
                this.displaySelectedStr += this.getValue(item, field) + " - ";
            }
        }
    }

    Search(key: string) {
        key = key.toLowerCase().trim();
        const constData = this.ConstDataSources;
        const displayFields = this.DisplayFields;
        const context = this;

        const data = constData.filter(o => {
            let matched: boolean = false;
            for (const i of displayFields) {
                const field: string = i.field;
                const value: string = context.getValue(o, field) == null ? "" : context.getValue(o, field);
                const valueType: string = typeof value;

                if (valueType === 'boolean' && value === key) {
                    matched = true;
                }

                if (valueType === 'string' && value.toLowerCase().includes(key)) {
                    matched = true;
                }

                if (valueType === 'number' && +value === +key) {
                    matched = true;
                }
            }
            return matched;
        });
        if (!!data.length) {
            this.DataSources = data;
        } else {
            this.DataSources = constData;
        }

        if (this.CurrentActiveItemIdObj !== null && this.CurrentActiveItemIdObj.value !== null) {
            const _CurrentActiveItemIdObj: { field: string, value: any, hardValue: any } = this.CurrentActiveItemIdObj;
            this.indexSelected = this.DataSources.findIndex(o => o[_CurrentActiveItemIdObj.field] === _CurrentActiveItemIdObj.value);
        }
    }

    getValue(item: any, field: string) {
        try {
            // if (field.includes(".")) {
            //     const fieldList = field.split(".");
            //     for (const i of fieldList) {
            //         item = item[i];
            //     }
            //     return item;
            // } else {
            //     return item[field];
            // }
            return item[field];

        } catch (error) {
            return null;
        }

    }

}
