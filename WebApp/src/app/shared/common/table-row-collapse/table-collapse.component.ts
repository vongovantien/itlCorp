import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { AppPage } from 'src/app/app.base';

@Component({
    selector: 'table-collapse',
    templateUrl: './table-collapse.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class TableCollapseComponent extends AppPage {

    @Input() show: boolean = true;
    @Input() id: string = '';
    constructor() {
        super();
    }

    ngOnInit() {
        this.subscribseStateCollapse().subscribe(i => console.log(i));
    }
}
