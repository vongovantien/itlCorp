import { Component, Input, Output, EventEmitter, ElementRef, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { DataService } from '@services';

@Component({
    selector: 'app-combo-grid',
    templateUrl: './app-combo-grid.component.html',
    // changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComboGridComponent<T> extends AppList {
    @ViewChild('inputSearch', { static: true }) inputSearch: ElementRef;

    @Input() headers: CommonInterface.IHeaderTable[];
    @Input() data: T[] = [];
    @Input() height: 200;
    @Input() fields: string[] = [];
    @Output() onClick: EventEmitter<any> = new EventEmitter<any>();
    @Input() active: any;

    @Input() set isEvent(v: boolean) {
        this.isUsinggDataService = v;
    }
    selectedItem: any = null;

    isUsinggDataService: boolean = true;
    constructor(
        private _dataService: DataService
    ) {
        super();
    }

    ngOnInit(): void {
        if (this.inputSearch) {
            setTimeout(() => this.inputSearch.nativeElement.focus(), 0);
        }
    }

    selectItem(item: any) {
        this.active = item.id;
        this.selectedItem = item;
        this.onClick.emit(this.selectedItem);

        if (this.isUsinggDataService) {
            this._dataService.$data.next(this.selectedItem);
        }
    }

    clickSearch($event: Event) {
        $event.stopPropagation();
        $event.stopImmediatePropagation();
    }
}
