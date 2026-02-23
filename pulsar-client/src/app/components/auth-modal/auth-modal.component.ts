import { Component, EventEmitter, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-auth-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './auth-modal.component.html',
  styleUrl: './auth-modal.component.scss',
})
export class AuthModalComponent {
  @Output() closed = new EventEmitter<void>();
  @Output() success = new EventEmitter<void>();

  mode: 'login' | 'register' = 'login';
  email = '';
  password = '';
  error = '';
  loading = false;

  constructor(private auth: AuthService) {}

  async submit() {
    this.error = '';
    this.loading = true;
    const call =
      this.mode === 'login'
        ? this.auth.login(this.email, this.password)
        : this.auth.register(this.email, this.password);

    call.subscribe({
      next: (res) => {
        this.auth.saveSession(res.token, res.displayName);
        this.loading = false;
        this.success.emit();
        this.closed.emit();
      },
      error: (err) => {
        this.error = err.error?.message || 'Something went wrong';
        this.loading = false;
      },
    });
  }

  toggle() {
    this.mode = this.mode === 'login' ? 'register' : 'login';
    this.error = '';
  }
}
