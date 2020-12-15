import { Component, OnInit, Input, ChangeDetectionStrategy, Output, EventEmitter } from '@angular/core';
import { Store } from '@ngrx/store';
import { IAppState, getMenuUserPermissionState } from '@store';
import { AppForm } from 'src/app/app.form';
import { Observable } from 'rxjs';

@Component({
    selector: 'app-permission-button',
    templateUrl: './permission-button.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppPermissionButtonComponent extends AppForm implements OnInit {
    @Input() title: string;
    @Input() set type(type: string) { this._type = type; }

    @Input() set class(c: string) { this._class = c; }

    @Input() set icon(i: string) { this._icon = i; }

    get type() { return this._type; }

    get class() { return this._class; }

    get icon() { return this._icon; }

    @Output() onClick: EventEmitter<any> = new EventEmitter<any>();

    private _type: string;
    private _class: string;
    private _icon: string = 'la la-plus';

    menuPermission: Observable<SystemInterface.IUserPermission>;

    constructor(
        private _store: Store<IAppState>
    ) {
        super();
    }

    ngOnInit(): void {
        this.menuPermission = this._store.select(getMenuUserPermissionState);
    }


    onClicked(e) {
        this.onClick.emit(e);
    }
}
