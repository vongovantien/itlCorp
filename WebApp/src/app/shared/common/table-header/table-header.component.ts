import { Component, Input } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: '[app-table-header]',
    templateUrl: './table-header.component.html'
})

export class TableHeaderComponent extends AppList {
    @Input() headers: CommonInterface.IHeaderTable[] = [];
    constructor() { 
        super();
    }

    ngOnInit() { }
}
