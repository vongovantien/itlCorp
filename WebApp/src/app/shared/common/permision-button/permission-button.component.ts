import { Component, OnInit, Input, ChangeDetectionStrategy } from '@angular/core';
import { Store } from '@ngrx/store';
import { IAppState, getMenuUserPermissionState } from '@store';
import { AppForm } from 'src/app/app.form';
import { Observable } from 'rxjs';


@Component({
    selector: 'app-permission-button',
    templateUrl: './permission-button.component.html',
})
export class AppPermissionButtonComponent extends AppForm implements OnInit {
    @Input() title: string = 'new';
    @Input() type: string = 'add';

    menuPermission: Observable<SystemInterface.IUserPermission>;

    constructor(
        private _store: Store<IAppState>
    ) {
        super();

    }

    ngOnInit(): void {
        this.menuPermission = this._store.select(getMenuUserPermissionState);
    }
}
