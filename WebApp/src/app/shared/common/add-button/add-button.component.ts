import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { Store } from '@ngrx/store';
import { IAppState, getMenuUserPermissionState } from '@store';
import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'app-add-button',
    templateUrl: './add-button.component.html',
})
export class AppAddButtonComponent extends AppForm implements OnInit {
    @Output() click: EventEmitter<any> = new EventEmitter<any>();
    @Input() title: string;

    menuPermission: Observable<SystemInterface.IUserPermission>;
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
                        this.isShow = res.allowAdd;
                    }
                }
            );
    }

    onClickAdd() {
        this.click.emit();
    }
}
