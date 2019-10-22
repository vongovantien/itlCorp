import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-combo-grid',
    templateUrl: './app-combo-grid.component.html',
    // changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComboGridComponent<T> extends AppList {

    @Input() headers: CommonInterface.IHeaderTable[];
    @Input() data: any = [];
    @Input() height: 200;
    @Output() onClick: EventEmitter<any> = new EventEmitter<any>();

    selectedItem: any = null;

    constructor() {
        super();
    }

    ngOnInit(): void { }

    selectItem(item: any) {
        this.selectedItem = item;
        this.onClick.emit(this.selectedItem);
    }
}
