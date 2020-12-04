import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChanges, AfterViewInit, ElementRef, ViewChild, QueryList, ViewChildren, ChangeDetectionStrategy, HostListener } from '@angular/core';
import { AppPage } from 'src/app/app.base';

import cloneDeep from 'lodash/cloneDeep';
import { ListKeyManager, FocusKeyManager } from '@angular/cdk/a11y';
import { DOWN_ARROW, ENTER } from '@angular/cdk/keycodes';
import { AppCombogridItemComponent } from './combogrid-item/combo-grid-item.component';
@Component({
    selector: 'app-combo-grid-virtual-scroll',
    templateUrl: './combo-grid-virtual-scroll.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ComboGridVirtualScrollComponent extends AppPage implements OnInit, OnChanges, AfterViewInit {

    @Input() dataSources: any[];
    @Input() displayFields: { field: string, label: string }[];
    @Input() searchFields: any[];
    @Input() selectedDisplayFields: any[];
    @Input() currentActiveItemId: any;
    @Input() disabled: boolean;
    @Input() height: number = 200;
    @Input() isTooltip: boolean = false;
    @Input() loading: boolean = false;
    @Input() placeholder: string = '';
    @Input() size: number = 25;

    @Output() itemSelected = new EventEmitter<any>();
    @Output() remove = new EventEmitter<any>();

    @ViewChild('inputSearch') inputSearch: ElementRef;
    @ViewChild('clkSearch', { static: true }) inputPlaceholder: ElementRef;

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
    searchKeys: string[] = [];
    showAngledownIcon: boolean = false;

    keyboardEventsManager: ListKeyManager<any>;

    isFocusSearch: boolean = false;

    @ViewChildren(AppCombogridItemComponent) items: QueryList<AppCombogridItemComponent>;
    private keyManager: FocusKeyManager<AppCombogridItemComponent>;

    @HostListener('keydown.Enter', ['$event'])
    onKeydownEnterHandler(event: KeyboardEvent) {
        if (this.isFocusSearch) {
            this.inputPlaceholder.nativeElement.click();
        }
    }

    constructor(
    ) {
        super();
    }

    ngAfterViewInit() {
        this.keyManager = new FocusKeyManager(this.items).withWrap();
    }

    onKeydown(event: any) {
        if (event.keyCode === ENTER) {
            const item = this.keyManager.activeItem.data;

            this.itemSelected.emit(item);
            this.setCurrentActiveItem(item);

            this.currentItemSelected = item;
            if (this.CurrentActiveItemIdObj !== null && this.CurrentActiveItemIdObj.value !== null) {
                this.CurrentActiveItemIdObj.value = item[this.CurrentActiveItemIdObj.field];
            }

            // * Reset keyword search.
            this.keyword = '';
        } else {
            this.keyManager.onKeydown(event);
        }
    }

    onKeydownSearchInput(e: any) {
        if (e.keyCode === DOWN_ARROW) {
            this.keyManager.onKeydown(e);
            this.keyManager.setFirstItemActive();

            // if (!this.CurrentActiveItemIdObj) {
            //     this.keyManager.setFirstItemActive();
            // } else {
            //     const itemIndex = this.ConstDataSources.findIndex(o => o[this.CurrentActiveItemIdObj.field] === this.CurrentActiveItemIdObj.value);
            //     if (itemIndex !== -1) {
            //         this.keyManager.setActiveItem(itemIndex);
            //     }
            // }
        }
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (!!changes.dataSources) {
            this.loading = changes.dataSources.firstChange;
            if (!!changes.dataSources.currentValue) {
                if (!!changes.dataSources.currentValue.length) {
                    if (changes.dataSources.firstChange) {
                        this.loading = false;
                    }
                } else {
                    this.loading = false;
                }
                this.setDataSource(changes.dataSources.currentValue);
            } else {
                this.loading = false;
            }
        }
        if (!!changes.displayFields && !!changes.displayFields.currentValue) {
            this.setDisplayFields(changes.displayFields.currentValue);
        }
        if (!!changes.selectedDisplayFields && !!changes.selectedDisplayFields.currentValue) {
            this.setSelectedDisplayFields(changes.selectedDisplayFields.currentValue);
        }
        if (!!changes.currentActiveItemId && (!!changes.currentActiveItemId.currentValue)) {
            this.setCurrentActiveItemId(changes.currentActiveItemId.currentValue);
        }
        if (changes.disabled !== undefined && changes.disabled !== null) {
            this.setDisabled(changes.disabled.currentValue);
        }
        if (!!changes.loading && typeof changes.loading.currentValue === 'boolean') {
            this.loading = changes.loading.currentValue;
        }
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
                    this.setCurrentActiveItem(this.ConstDataSources[itemIndex]);
                } else if (!!activeItemData.hardValue) {
                    this.displaySelectedStr = activeItemData.hardValue;
                }
            }
        } else {
            this.DataSources = data;
            this.ConstDataSources = cloneDeep(data);
        }
    }

    setDisplayFields(data: { field: string, label: string }[]) {
        if (!!data && data.length > 0) {
            this.DisplayFields = data;
            this.searchKeys = this.DisplayFields.map(d => d.field);
        }
    }

    setSelectedDisplayFields(data: any[]) {
        if (data.length > 0) {
            this.SelectedDisplayFields = data;
            this.searchKeys.push(...data);
        }
    }

    setDisabled(data: boolean) {
        this.Disabled = data;
    }

    setCurrentActiveItemId(data: any) {
        if (data.value != null) {
            this.CurrentActiveItemIdObj = data;
            if (!!this.ConstDataSources.length) {
                const itemIndex = this.ConstDataSources.findIndex(i => i[data.field] === data.value);
                if (itemIndex !== -1) {
                    this.indexSelected = itemIndex;
                    this.setCurrentActiveItem(this.ConstDataSources[itemIndex]);
                } else if (!!data.hardValue) {
                    this.displaySelectedStr = data.hardValue;
                }
            }

        } else {
            this.indexSelected = -1;
            if (!!data.hardValue) {
                this.displaySelectedStr = data.hardValue;
            } else {
                this.displaySelectedStr = null;
            }
        }
    }

    removeCurrenActiveItem() {
        // this.indexSelected = -1;
        // this.displaySelectedStr = null;
        this.setCurrentActiveItemId({ value: null });
        this.remove.emit();
    }

    emitSelected(item: any, index: number) {
        this.indexSelected = index;
        this.itemSelected.emit(item);
        this.setCurrentActiveItem(item);

        this.currentItemSelected = item;
        if (this.CurrentActiveItemIdObj !== null && this.CurrentActiveItemIdObj.value !== null) {
            this.CurrentActiveItemIdObj.value = item[this.CurrentActiveItemIdObj.field];
        }

        this.keyManager.setActiveItem(index);
        // * Reset keyword search.
        this.keyword = '';
    }

    setCurrentActiveItem(item: any) {
        this.displaySelectedStr = '';
        if (this.SelectedDisplayFields.length === 1) {
            this.displaySelectedStr += item[this.SelectedDisplayFields[0]];
        } else {
            let dataItem = '';
            this.SelectedDisplayFields.forEach((data: string, index: number) => {
                if (index === this.selectedDisplayFields.length - 1) {
                    this.displaySelectedStr += item[data];
                } else {
                    if (item[data] && item[data].trim()) {
                        dataItem += item[data] + ' - ';
                    }
                    this.displaySelectedStr = dataItem;
                }
            });
        }
    }

    clickSearch() {
        if (this.inputSearch) {
            setTimeout(() => this.inputSearch.nativeElement.focus(), 0);
        }
    }

    onFocusInputPlaceholder(e) {
        this.isFocusSearch = true;
    }
}
