import { Component, Input, Output, EventEmitter, forwardRef, ChangeDetectionStrategy } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
    selector: 'app-switch',
    templateUrl: './switch-toggle.component.html',
    styles: [
        `
        .m-switch.m-switch--success:not(.m-switch--outline) input:empty ~ span:before {
            background-color: #ebedf2;
        }
        .m-switch.m-switch--success:not(.m-switch--outline)
            input:checked
            ~ span:before {
            background-color: #34bfa3;
        }`
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            multi: true,
            useExisting: forwardRef(() => SwitchToggleComponent),
        }
    ]
})
export class SwitchToggleComponent implements ControlValueAccessor {

    private toggle: boolean = false; // * internal state

    @Input() class: string = 'success';
    @Input() size: string = "md";
    @Input() disabled: boolean = null;
    @Output() toggleChange: EventEmitter<boolean> = new EventEmitter<boolean>();

    private onChange: Function = (v: boolean) => {
    };
    private onTouch: Function = () => {
    };

    constructor() { }

    set toggleValue(val: boolean) {
        if (val !== null && val !== undefined) {
            this.toggle = val;

            this.onChange(val);
            this.onTouch(val);

            this.toggleChange.emit(this.toggle);
        }
    }

    get toggleValue() {
        return this.toggle;
    }

    public writeValue(value: any) {
        this.toggleValue = value;
    }

    public registerOnChange(fn: Function) {
        this.onChange = fn;
    }

    public registerOnTouched(fn: Function) {
        this.onTouch = fn;
    }

    public setDisabledState?(isDisabled: boolean): void {
        this.disabled = isDisabled;
    }

    ngOnInit(): void { }
}
