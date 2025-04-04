using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header ("Health")]
    [SerializeField] private float startingHealth; // Máu ban đầu
    public float currentHealth { get; private set; } // Máu hiện tại
    private Animator anim;
    public bool dead { get; private set; } // Cho phép đọc nhưng không cho phép ghi từ bên ngoài

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration; // Thời gian bất tử sau khi bị đánh
    [SerializeField] private int numberOfFlashes; // Số lần nhấp nháy khi bất tử
    private SpriteRenderer spriteRend;

    [Header("Components")]
    [SerializeField] private Behaviour[] components; // Các thành phần bị vô hiệu hóa khi chết
    private bool invulnerable; // Trạng thái bất tử tạm thời

    [Header("Death Sound")]
    [SerializeField] private AudioClip deathSound; // Âm thanh khi chết
    [SerializeField] private AudioClip hurtSound; // Âm thanh khi bị thương
    
    private Rigidbody2D rb;


    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); // Lấy Rigidbody2D

    }

    [System.Obsolete]
    public void TakeDamage(float _damage)
    {
        if (invulnerable) return; // Nếu đang bất tử, bỏ qua sát thương
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth); // Giới hạn máu từ 0 đến max

        if (currentHealth > 0)
        {
            anim.SetTrigger("hurt"); // Chạy animation bị thương
            StartCoroutine(Invunerability()); // Kích hoạt iFrames (bất tử tạm thời)
            SoundManager.instance.PlaySound(hurtSound); // Phát âm thanh bị thương
        }
        else
        {
            Die();
        }
    }

    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth); // Hồi máu nhưng không vượt quá giới hạn
    }

    private IEnumerator Invunerability()
    {
        invulnerable = true;
        Physics2D.IgnoreLayerCollision(10, 11, true); // Bỏ qua va chạm với kẻ địch
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRend.color = new Color(1, 0, 0, 0.5f); // Làm nhấp nháy màu đỏ
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            spriteRend.color = Color.white; // Trả về màu bình thường
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
        }
        Physics2D.IgnoreLayerCollision(10, 11, false); // Bật lại va chạm với kẻ địch
        invulnerable = false;
    }

    private void Deactivate()
    {
        gameObject.SetActive(false); // Ẩn GameObject khi chết
    }

    // Hồi sinh nhân vật
    public void Respawn()
{
    if (!dead) return; // Nếu chưa chết thì không cần hồi sinh

    dead = false; // Đánh dấu nhân vật không còn chết
    AddHealth(startingHealth);
    anim.ResetTrigger("die");
    anim.Play("Idle");

    foreach (Behaviour component in components)
        component.enabled = true;

    StartCoroutine(Invunerability()); // Kích hoạt trạng thái bất tử tạm thời
}

    [System.Obsolete]
    private void Die()
{
    if (dead) return; // Nếu đã chết thì không làm gì nữa

    dead = true;

    // Tắt tất cả thành phần ngoại trừ Rigidbody2D để tránh lỗi rơi tự do
    foreach (Behaviour component in components)
        component.enabled = false;

    rb.velocity = Vector2.zero; // Dừng di chuyển khi chết
    rb.gravityScale = 1; // Ngăn nhân vật tiếp tục rơi

    anim.SetTrigger("die"); // Chạy animation chết
    SoundManager.instance.PlaySound(deathSound);
}

}
