import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { Store } from '@ngrx/store';
import { IAppState, getMenuUserPermissionState } from '@store';
import { takeUntil } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'app-permission-button',
    templateUrl: './permission-button.component.html',
})
export class AppPermissionButtonComponent extends AppForm implements OnInit {
    @Output() click: EventEmitter<any> = new EventEmitter<any>();
    @Input() title: string = 'new';
    @Input() type: string = 'add';

    menuPermission: SystemInterface.IUserPermission;
    isShow: boolean = false;

    constructor(
        private _store: Store<IAppState>
    ) {
        super();

    }

    ngOnInit(): void {
        this._store.select(getMenuUserPermissionState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: SystemInterface.IUserPermission) => {
                    if (res !== null && res !== undefined) {
                        this.menuPermission = res;
                        this.isShow = res.allowAdd;
                    }
                }
            );
    }

    onClick() {
        this.click.emit();
    }
}
