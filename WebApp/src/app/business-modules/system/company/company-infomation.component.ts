import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-company-info',
    templateUrl: './company-infomation.component.html',
})
export class ComanyInfomationComponent extends AppList {

    constructor() {
        super();
    }

    ngOnInit() {
    }

}

