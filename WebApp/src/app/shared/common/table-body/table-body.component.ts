import { Component, Input, Output, EventEmitter } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { CommonEnum } from '../../enums/common.enum';

@Component({
    selector: '[app-table-body]',
    templateUrl: './table-body.component.html',
})
export class TableBodyComponent extends AppPage {

    @Input() data: any[] = [];
    @Input() headers: CommonInterface.IHeaderTable[] = [];
    @Input() actions: CommonEnum.TableActions[] = [];

    @Output() onClickDelete: EventEmitter<any> = new EventEmitter<any>();
    @Output() onClickEdit: EventEmitter<any> = new EventEmitter<any>();
    @Output() onClickLink: EventEmitter<any> = new EventEmitter<any>();

    constructor() {
        super();
    }

    ngOnInit(): void { }

    delete(item: any) {
        this.onClickDelete.emit(item);
    }

    edit(item: any) {
        this.onClickEdit.emit(item);
    }

    link(item: any) {
        this.onClickLink.emit(item);
    }

}
