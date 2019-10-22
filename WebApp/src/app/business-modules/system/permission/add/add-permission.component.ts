import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'app-create-permission',
    templateUrl: './add-permission.component.html',
})
export class PermissionCreateComponent extends AppForm {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
